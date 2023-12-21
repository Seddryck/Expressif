using Expressif.Parsers;
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
        stringBuilder.Append(function.Name.ToKebabCase());
        if (function.Parameters.Any())
        {
            stringBuilder.Append('(');
            foreach (var parameter in function.Parameters)
            {
                stringBuilder.Append(ParameterSerializer.Serialize(parameter switch
                {
                    IParameter p => p,
                    _ => new LiteralParameter(parameter?.ToString() ?? new Null().Keyword)
                }));
                stringBuilder.Append(',').Append(' ');
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(')');
        }
    }
}
