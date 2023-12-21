using Expressif.Parsers;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Serializers;

public class PredicationSerializerTest
{
    [Test]
    public void Serialize_SingleMember_NoPipe()
    {
        var predication = new SinglePredication(new Function("even", []));
        Assert.That(new PredicationSerializer().Serialize(predication), Is.EqualTo("even"));
    }

    [Test]
    public void Serialize_SingleParameter_SinglePredicationMemberSerializerCall()
    {
        var internalSerializer = new Mock<SinglePredicationSerializer>();

        var predication = new SinglePredication(new Function("even", []));
        var serializer = new PredicationSerializer(internalSerializer.Object);
        serializer.Serialize(predication);

        internalSerializer.Verify(x => x.Serialize(predication, ref It.Ref<StringBuilder>.IsAny), Times.Once);
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
    public void Serialize_MultipleMembers_MultiplePredicationMemberSerializerCall()
    {
        var sb = new StringBuilder();
        var internalSerializer = new Mock<SinglePredicationSerializer>();

        var Predication = new PredicationBuilder();
        var evenPredication = new SinglePredication(new Function("even", []));
        var greaterThanPredication = new SinglePredication(new Function("GreaterThan", [new LiteralParameter("5")]));
        var moduloPredication = new SinglePredication(new Function("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var compositePredication = new BinaryPredication(
            BinaryOperator.And
            , new BinaryPredication(BinaryOperator.Or, evenPredication, greaterThanPredication)
            , moduloPredication);
        var serializer = new PredicationSerializer(internalSerializer.Object);
        serializer.Serialize(compositePredication);

        internalSerializer.Verify(x => x.Serialize(It.IsAny<SinglePredication>(), ref It.Ref<StringBuilder>.IsAny), Times.Exactly(3));
    }

    [Test]
    public void Serialize_WithSubPredication_WithPipe()
    {
        var Predication = new PredicationBuilder();
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
