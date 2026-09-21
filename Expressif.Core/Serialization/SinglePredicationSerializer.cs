using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Bindings;

namespace Expressif.Serialization;

internal sealed class SinglePredicationSerializer
{
    private ParameterSerializer ParameterSerializer { get; } = new();

    public SinglePredicationSerializer() { }

    internal string Serialize(SinglePredication predication)
    {
        var stringBuilder = new StringBuilder();
        Serialize(predication, ref stringBuilder);
        return stringBuilder.ToString();
    }

    public void Serialize(SinglePredication predication, ref StringBuilder stringBuilder)
        => Serialize(predication.Member, ref stringBuilder);

    private void Serialize(Function predicate, ref StringBuilder stringBuilder)
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
