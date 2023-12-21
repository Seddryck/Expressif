using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serializers;

public class PredicationSerializer
{
    private SinglePredicationSerializer SingleSerializer { get; set; }

    public PredicationSerializer()
        : this(new()) { }
    public PredicationSerializer(SinglePredicationSerializer? singleSerializer = null)
        => SingleSerializer = singleSerializer ?? new();

    public virtual string Serialize(IPredication predication)
    {
        var stringBuilder = new StringBuilder();
        Serialize(predication, ref stringBuilder);
        return stringBuilder.ToString();
    }

    protected virtual void Serialize(IPredication predication, ref StringBuilder stringBuilder)
    {
        switch (predication)
        {
            case SinglePredication single:
                SingleSerializer.Serialize(single, ref stringBuilder);
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
                throw new NotImplementedException();
        };
    }
}
