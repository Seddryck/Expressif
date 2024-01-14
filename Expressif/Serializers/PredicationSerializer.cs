using Expressif.Parsers;
using Expressif.Predicates;
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

    public virtual string Serialize(IPredicationBuilder builder)
    {
        var stringBuilder = new StringBuilder();
        Serialize(builder, ref stringBuilder);
        return stringBuilder.ToString();
    }

    protected virtual void Serialize(IPredicationBuilder builder, ref StringBuilder stringBuilder)
        => Serialize(builder.Pile ?? throw new InvalidOperationException(), ref stringBuilder);

    public virtual string Serialize(IPredicationParsable predication)
    {
        var stringBuilder = new StringBuilder();
        Serialize(predication, ref stringBuilder);
        return stringBuilder.ToString();
    }

    protected virtual void Serialize(IPredicationParsable predication, ref StringBuilder stringBuilder)
    {
        switch (predication)
        {
            case SinglePredicationMeta single:
                SingleSerializer.Serialize(single, ref stringBuilder);
                break;
            case UnaryPredicationMeta unary:
                stringBuilder.Append('!');
                stringBuilder.Append('{');
                Serialize(unary.Member, ref stringBuilder);
                stringBuilder.Append('}');
                break;
            case BinaryPredicationMeta binary:
                stringBuilder.Append('{');
                Serialize(binary.Left, ref stringBuilder);
                stringBuilder
                    .Append(' ')
                    .Append('|')
                    .Append(binary.Operator.Name.ToUpperInvariant())
                    .Append(' ');
                Serialize(binary.Right, ref stringBuilder);
                stringBuilder.Append('}');
                break;
            default:
                throw new NotImplementedException();
        };
    }
}
