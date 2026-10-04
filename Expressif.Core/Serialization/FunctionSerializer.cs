using Expressif.Bindings;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serialization;

internal sealed class FunctionSerializer
{
    private ParameterSerializer ParameterSerializer { get; }

    public FunctionSerializer()
        : this(new ParameterSerializer()) { }

    internal FunctionSerializer(ParameterSerializer parameterSerializer)
        => ParameterSerializer = parameterSerializer;

    public string Serialize(Function function)
    {
        var stringBuilder = new StringBuilder();
        Serialize(function, ref stringBuilder);
        return stringBuilder.ToString();
    }

    public void Serialize(Function function, ref StringBuilder stringBuilder)
    {
        if (TrySerializeReference(function, stringBuilder))
            return;
        if (function.Role is BoundFunctionRole.ConditionalForward or BoundFunctionRole.ConditionalBackward)
            SerializeConditional(function, stringBuilder);
        else if (function.Name is "switch" or "try")
            SerializeBranches(function, stringBuilder);
        else if (function.Notation is SourceNotation.InputFieldShorthand or SourceNotation.CurrentField
            or SourceNotation.RootField or SourceNotation.EnclosingRootField)
            SerializeField(function, stringBuilder);
        else
            SerializeCall(function, stringBuilder);
    }

    private void SerializeConditional(Function function, StringBuilder output)
        => output.Append('(').Append(ParameterSerializer.Serialize(function.Parameters[0])).Append(')')
            .Append(function.Role == BoundFunctionRole.ConditionalForward ? " ?> " : " <? ")
            .Append('(').Append(ParameterSerializer.Serialize(function.Parameters[1])).Append(')');

    private void SerializeBranches(Function function, StringBuilder output)
    {
        output.Append(function.Name).Append('(');
        output.Append(string.Join(", ", function.Parameters.Cast<ControlFlowBranchParameter>()
            .Select(branch => SerializeBranch(branch, function.Name == "try"))));
        output.Append(')');
    }

    private string SerializeBranch(ControlFlowBranchParameter branch, bool isTry)
    {
        var expression = ParameterSerializer.Serialize(branch.Expression);
        if (branch.Predicate is null)
            return $"_ => {expression}";
        var predicate = ParameterSerializer.Serialize(branch.Predicate);
        return isTry ? $"{expression} => {predicate}" : $"{predicate} => {expression}";
    }

    private static void SerializeField(Function function, StringBuilder output)
    {
        var prefix = function.Notation switch
        {
            SourceNotation.RootField => "^.",
            SourceNotation.EnclosingRootField => "^^.",
            _ => ".",
        };
        var name = function.Parameters.Single() switch
        {
            LiteralParameter literal => literal.Value,
            QuotedLiteralParameter quoted => quoted.Value,
            _ => throw new NotSupportedException(),
        };
        output.Append(prefix).Append(name);
    }

    private void SerializeCall(Function function, StringBuilder output)
    {
        output.Append(function.Name.ToKebabCase());
        if (function.Parameters.Count == 0)
            return;
        output.Append('(');
        foreach (var argument in function.Arguments)
        {
            if (argument.Name is not null)
                output.Append(argument.Name).Append(" := ");
            if (argument.IsSpread)
                output.Append("...");
            output.Append(ParameterSerializer.Serialize(argument.Value));
            output.Append(',').Append(' ');
        }
        output.Remove(output.Length - 2, 2);
        output.Append(')');
    }

    private bool TrySerializeReference(Function function, StringBuilder stringBuilder)
    {
        if (function.Notation == SourceNotation.InputTupleProjectionShorthand
            && function.Parameters is [TupleProjectionParameter projection])
        {
            stringBuilder.Append('$');
            if (projection.FromEnd)
                stringBuilder.Append('^');
            stringBuilder.Append(projection.Index);
            return true;
        }
        if (function.Notation == SourceNotation.ScopedTupleProjectionShorthand)
        {
            stringBuilder.Append(ParameterSerializer.Serialize(function.Parameters.Single()));
            return true;
        }
        return false;
    }
}
