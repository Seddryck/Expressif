using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Bindings;
using Expressif.Predicates.Operators;

namespace Expressif.Serializers;

public class SinglePredicationSerializer
{
    private ParameterSerializer ParameterSerializer { get; set; }

    public SinglePredicationSerializer()
        : this(new()) { }
    public SinglePredicationSerializer(ParameterSerializer? parameterSerializer = null)
        => ParameterSerializer = parameterSerializer ?? new();

    internal virtual string Serialize(SinglePredication predication)
    {
        var stringBuilder = new StringBuilder();
        Serialize(predication, ref stringBuilder);
        return stringBuilder.ToString();
    }

    public virtual void Serialize(SinglePredication predication, ref StringBuilder stringBuilder)
        => Serialize(predication.Member, ref stringBuilder);

    protected virtual void Serialize(Function predicate, ref StringBuilder stringBuilder)
    {
        stringBuilder.Append(predicate.Name.ToKebabCase());
        if (predicate.Parameters.Length != 0)
        {
            stringBuilder.Append('(');
            foreach (var argument in predicate.Arguments)
            {
                if (argument.Name is not null)
                    stringBuilder.Append(argument.Name).Append(" := ");
                stringBuilder.Append(ParameterSerializer.Serialize(argument.Value));
                stringBuilder.Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(')');
        }
    }
}
