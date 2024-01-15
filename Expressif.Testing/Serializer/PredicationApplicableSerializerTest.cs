using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates;
using Expressif.Predicates.Operators;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Expressif.Testing.Serializers;

public class PredicationApplicableSerializerTest
{
    [Test]
    public void Serialize_SingleMember_NoPipe()
    {
        var predication = new SinglePredicationMeta(new FunctionMeta("even", []));
        var applicable = new PredicationApplicableMeta(new LiteralParameter("105"), predication);
        Assert.That(new PredicationApplicableSerializer().Serialize(applicable), Is.EqualTo("105 |? even"));
    }

    [Test]
    public void Serialize_BinaryMember_NoPipe()
    {
        var predication = new BinaryPredicationMeta
        (
            new OperatorMeta("OR"),
            new SinglePredicationMeta(new FunctionMeta("even", [])),
            new SinglePredicationMeta(new FunctionMeta("greater-than", [new LiteralParameter("100")]))
        );
        var applicable = new PredicationApplicableMeta(new LiteralParameter("105"), predication);
        Assert.That(new PredicationApplicableSerializer().Serialize(applicable), Is.EqualTo("105 |? {even |OR greater-than(100)}"));
    }
}
