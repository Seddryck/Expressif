using Expressif.Serialization;
using Expressif.Bindings;

namespace Expressif.Testing.Serialization;

public class SinglePredicationSerializerTest
{
    [Test]
    public void Serialize_NoParameter_NoParenthesis()
    {
        var single = new SinglePredication(new Function("Even", []));
        Assert.That(new SinglePredicationSerializer().Serialize(single), Is.EqualTo("even"));
    }

    [Test]
    public void Serialize_WithSingleParameter_Parenthesis()
    {
        var single = new SinglePredication(new Function("GreaterThan", [new LiteralParameter("5")]));
        Assert.That(new SinglePredicationSerializer().Serialize(single), Is.EqualTo("greater-than(5)"));
    }

    [Test]
    public void Serialize_MultipleParameter_ParenthesisAndComas()
    {
        var single = new SinglePredication(new Function("Modulo", [new LiteralParameter("7"), new LiteralParameter("3")]));
        Assert.That(new SinglePredicationSerializer().Serialize(single), Is.EqualTo("modulo(7, 3)"));
    }
}
