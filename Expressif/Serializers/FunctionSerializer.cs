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
        if (function.Syntax == FunctionSyntax.FieldShorthand)
        {
            stringBuilder.Append('.').Append(((LiteralParameter)function.Parameters.Single()).Value);
            return;
        }

        stringBuilder.Append(function.Name.ToKebabCase());
        if (function.Parameters.Any())
        {
            stringBuilder.Append('(');
            foreach (var argument in function.Arguments)
            {
                if (argument.Name is not null)
                    stringBuilder.Append(argument.Name).Append(" := ");
                if (argument.IsSpread)
                    stringBuilder.Append("...");
                stringBuilder.Append(ParameterSerializer.Serialize(argument.Value switch
                {
                    IParameter p => p,
                    _ => new LiteralParameter(argument.Value?.ToString() ?? new Null().Keyword)
                }));
                stringBuilder.Append(',').Append(' ');
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(')');
        }
    }
}
