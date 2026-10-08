using Expressif.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serialization;

internal sealed class PredicationSerializer
{
    private SinglePredicationSerializer SingleSerializer { get; } = new();

    public PredicationSerializer() { }

    public string Serialize(IPredication predication)
    {
        var stringBuilder = new StringBuilder();
        Serialize(predication, ref stringBuilder);
        return stringBuilder.ToString();
    }

    private void Serialize(IPredication predication, ref StringBuilder stringBuilder)
    {
        switch (predication)
        {
            case SinglePredication single:
                SingleSerializer.Serialize(single, ref stringBuilder);
                break;
            case PipelinePredication pipeline:
                var serializer = new FunctionSerializer();
                stringBuilder.Append(string.Join(" | ", pipeline.Expression.Members.Select(serializer.Serialize)));
                break;
            case UnaryPredication unary:
                stringBuilder.Append('!');
                stringBuilder.Append('{');
                Serialize(unary.Member, ref stringBuilder);
                stringBuilder.Append('}');
                break;
            case BinaryPredication binary:
                stringBuilder.Append('{');
                Serialize(binary.LeftMember, ref stringBuilder);
                stringBuilder
                    .Append(' ')
                    .Append('|')
                    .Append(binary.Operator.Name.ToUpperInvariant())
                    .Append(' ');
                Serialize(binary.RightMember, ref stringBuilder);
                stringBuilder.Append('}');
                break;
            default:
                throw new BindingException($"Unsupported predication model '{predication.GetType().Name}'.");
        }
    }
}
