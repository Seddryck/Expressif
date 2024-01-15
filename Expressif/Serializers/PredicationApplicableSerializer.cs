using Expressif.Parsers;
using Expressif.Predicates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serializers;

public class PredicationApplicableSerializer : PredicationSerializer
{
    private ParameterSerializer ParameterSerializer { get; }

    public PredicationApplicableSerializer(SinglePredicationSerializer? singleSerializer = null, ParameterSerializer? parameterSerializer = null)
        : base(singleSerializer)
    {
        ParameterSerializer = parameterSerializer ?? new();
    }

    protected override void Serialize(IPredicationBuilder builder, ref StringBuilder stringBuilder)
    {
        if (builder is IPredicationApplicableBuilder applicable)
        {
            ParameterSerializer.Serialize(applicable.Argument ?? throw new InvalidOperationException(), ref stringBuilder);
            stringBuilder.Append(" |? ");
        }
        base.Serialize(builder, ref stringBuilder);
    }

    protected override void Serialize(IPredicationParsable predication, ref StringBuilder stringBuilder)
    {
        if (predication is PredicationApplicableMeta meta)
        {
            ParameterSerializer.Serialize(meta.Argument ?? throw new InvalidOperationException(), ref stringBuilder);
            stringBuilder.Append(" |? ");
            base.Serialize(meta.Member, ref stringBuilder);
        }
        else
            base.Serialize(predication, ref stringBuilder);
    }
}
