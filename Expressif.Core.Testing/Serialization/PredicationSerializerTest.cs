using Expressif.Bindings;
using Expressif.Serialization;

namespace Expressif.Testing.Serialization;

public class PredicationSerializerTest
{
    [Test]
    public void Serialize_SingleMember_NoPipe()
    {
        var predication = new SinglePredication(new Function("even", []));
        Assert.That(new PredicationSerializer().Serialize(predication), Is.EqualTo("even"));
    }

    [Test]
    public void Serialize_MultipleMembers_WithPipe()
    {
        var evenPredication = new SinglePredication(new Function("even", []));
        var greaterThanPredication = new SinglePredication(new Function("GreaterThan", [new LiteralParameter("5")]));
        var moduloPredication = new SinglePredication(new Function("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var compositePredication = new BinaryPredication(
            BinaryOperator.And
            , new BinaryPredication(BinaryOperator.Or, evenPredication, greaterThanPredication)
            , moduloPredication);
        Assert.That(new PredicationSerializer().Serialize(compositePredication)
            , Is.EqualTo("{{even |OR greater-than(5)} |AND modulo(7, 3)}"));
    }

    [Test]
    public void Serialize_WithSubPredication_WithPipe()
    {
        var predication = new TestPredicationBuilder();
        var evenPredication = new SinglePredication(new Function("even", []));
        var greaterThanPredication = new SinglePredication(new Function("GreaterThan", [new LiteralParameter("5")]));
        var moduloPredication = new SinglePredication(new Function("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var zeroOrNullPredication = new SinglePredication(new Function("ZeroOrNull", []));
        var compositePredication = new BinaryPredication(
            BinaryOperator.And
            , evenPredication
            , new BinaryPredication(BinaryOperator.Or, greaterThanPredication, moduloPredication));
        var fullPredication = new BinaryPredication(
            BinaryOperator.Or
            , compositePredication
            , zeroOrNullPredication);

        Assert.That(new PredicationSerializer().Serialize(fullPredication)
            , Is.EqualTo("{{even |AND {greater-than(5) |OR modulo(7, 3)}} |OR zero-or-null}"));
    }
}
