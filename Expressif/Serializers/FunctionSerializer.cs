using Expressif.Bindings;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serializers;

public class FunctionSerializer
{
    private ParameterSerializer ParameterSerializer { get; }

    public FunctionSerializer()
        : this(new ParameterSerializer()) { }

    public FunctionSerializer(ParameterSerializer? parameterSerializer = null)
        => ParameterSerializer = parameterSerializer ?? new ParameterSerializer();

    public virtual string Serialize(Function function)
    {
        var stringBuilder = new StringBuilder();
        Serialize(function, ref stringBuilder);
        return stringBuilder.ToString();
    }

    public virtual void Serialize(Function function, ref StringBuilder stringBuilder)
    {
        if (function.Syntax == FunctionSyntax.ScopedTupleProjectionShorthand)
            stringBuilder.Append(ParameterSerializer.Serialize(function.Parameters.Single()));
        else if (function.Syntax is FunctionSyntax.ConditionalForward or FunctionSyntax.ConditionalBackward)
            SerializeConditional(function, stringBuilder);
        else if (function.Name is "switch" or "try")
            SerializeBranches(function, stringBuilder);
        else if (function.Syntax is FunctionSyntax.FieldShorthand
            or FunctionSyntax.RootFieldShorthand or FunctionSyntax.EnclosingRootFieldShorthand)
            SerializeField(function, stringBuilder);
        else
            SerializeCall(function, stringBuilder);
    }

    private void SerializeConditional(Function function, StringBuilder output)
        => output.Append('(').Append(ParameterSerializer.Serialize(function.Parameters[0])).Append(')')
            .Append(function.Syntax == FunctionSyntax.ConditionalForward ? " ?> " : " <? ")
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
        var prefix = function.Syntax switch
        {
            FunctionSyntax.RootFieldShorthand => "^.",
            FunctionSyntax.EnclosingRootFieldShorthand => "^^.",
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
        if (function.Parameters.Length == 0)
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
}
