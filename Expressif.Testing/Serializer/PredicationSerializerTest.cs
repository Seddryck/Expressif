using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates;
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
        var predication = new SinglePredicationMeta(new FunctionMeta("even", []));
        Assert.That(new PredicationSerializer().Serialize(predication), Is.EqualTo("even"));
    }

    [Test]
    public void Serialize_SingleParameter_SinglePredicationMemberSerializerCall()
    {
        var internalSerializer = new Mock<SinglePredicationSerializer>();

        var predication = new SinglePredicationMeta(new FunctionMeta("even", []));
        var serializer = new PredicationSerializer(internalSerializer.Object);
        serializer.Serialize(predication);

        internalSerializer.Verify(x => x.Serialize(predication, ref It.Ref<StringBuilder>.IsAny), Times.Once);
    }

    [Test]
    public void Serialize_MultipleMembers_WithPipe()
    {
        var evenPredication = new SinglePredicationMeta(new FunctionMeta("even", []));
        var greaterThanPredication = new SinglePredicationMeta(new FunctionMeta("GreaterThan", [new LiteralParameter("5")]));
        var moduloPredication = new SinglePredicationMeta(new FunctionMeta("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var compositePredication = new BinaryPredicationMeta(
            BinaryOperatorParser.And
            , new BinaryPredicationMeta(BinaryOperatorParser.Or, evenPredication, greaterThanPredication)
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
        var evenPredication = new SinglePredicationMeta(new FunctionMeta("even", []));
        var greaterThanPredication = new SinglePredicationMeta(new FunctionMeta("GreaterThan", [new LiteralParameter("5")]));
        var moduloPredication = new SinglePredicationMeta(new FunctionMeta("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var compositePredication = new BinaryPredicationMeta(
            BinaryOperatorParser.And
            , new BinaryPredicationMeta(BinaryOperatorParser.Or, evenPredication, greaterThanPredication)
            , moduloPredication);
        var serializer = new PredicationSerializer(internalSerializer.Object);
        serializer.Serialize(compositePredication);

        internalSerializer.Verify(x => x.Serialize(It.IsAny<SinglePredicationMeta>(), ref It.Ref<StringBuilder>.IsAny), Times.Exactly(3));
    }

    [Test]
    public void Serialize_WithSubPredication_WithPipe()
    {
        var Predication = new PredicationBuilder();
        var evenPredication = new SinglePredicationMeta(new FunctionMeta("even", []));
        var greaterThanPredication = new SinglePredicationMeta(new FunctionMeta("GreaterThan", [new LiteralParameter("5")]));
        var moduloPredication = new SinglePredicationMeta(new FunctionMeta("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var zeroOrNullPredication = new SinglePredicationMeta(new FunctionMeta("ZeroOrNull", []));
        var compositePredication = new BinaryPredicationMeta(
            BinaryOperatorParser.And
            , evenPredication
            , new BinaryPredicationMeta(BinaryOperatorParser.Or, greaterThanPredication, moduloPredication));
        var fullPredication = new BinaryPredicationMeta(
            BinaryOperatorParser.Or
            , compositePredication
            , zeroOrNullPredication);

        Assert.That(new PredicationSerializer().Serialize(fullPredication)
            , Is.EqualTo("{{even |AND {greater-than(5) |OR modulo(7, 3)}} |OR zero-or-null}"));
    }
}
