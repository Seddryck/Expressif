using Expressif.Serializers;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif.Testing.Serializers;

public class SinglePredicationSerializerTest
{
    [Test]
    public void Serialize_NoParameter_NoParenthesis()
    {
        var single = new SinglePredicationMeta(new FunctionMeta("Even", []));
        Assert.That(new SinglePredicationSerializer().Serialize(single), Is.EqualTo("even"));
    }

    [Test]
    public void Serialize_NoParameter_NoParameterSerializerCall()
    {
        var internalSerializer = new Mock<ParameterSerializer>();
        internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

        var single = new SinglePredicationMeta(new FunctionMeta("Even", []));
        var serializer = new SinglePredicationSerializer(parameterSerializer: internalSerializer.Object);
        serializer.Serialize(single);

        internalSerializer.Verify(x => x.Serialize(It.IsAny<IParameter>()), Times.Never);
    }

    [Test]
    public void Serialize_WithSingleParameter_Parenthesis()
    {
        var single = new SinglePredicationMeta(new FunctionMeta("GreaterThan", [new LiteralParameter("5")]));
        Assert.That(new SinglePredicationSerializer().Serialize(single), Is.EqualTo("greater-than(5)"));
    }

    [Test]
    public void Serialize_WithSingleParameter_OneParameterSerializerCall()
    {
        var internalSerializer = new Mock<ParameterSerializer>();
        internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

        var single = new SinglePredicationMeta(new FunctionMeta("GreaterThan", [new LiteralParameter("5")]));
        var serializer = new SinglePredicationSerializer(parameterSerializer: internalSerializer.Object);
        serializer.Serialize(single);

        internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Once);
    }

    [Test]
    public void Serialize_MultipleParameter_ParenthesisAndComas()
    {
        var single = new SinglePredicationMeta(new FunctionMeta("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        Assert.That(new SinglePredicationSerializer().Serialize(single), Is.EqualTo("modulo(7, 3)"));
    }

    [Test]
    public void Serialize_MultipleParameter_MultipleParameterSerializerCall()
    {
        var internalSerializer = new Mock<ParameterSerializer>();
        internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

        var single = new SinglePredicationMeta(new FunctionMeta("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        var serializer = new SinglePredicationSerializer(parameterSerializer: internalSerializer.Object);
        serializer.Serialize(single);

        internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Exactly(2));
    }
}
