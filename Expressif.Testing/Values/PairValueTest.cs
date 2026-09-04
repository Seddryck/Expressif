using Expressif.Values;

namespace Expressif.Testing.Values;

public class PairValueTest
{
    [Test]
    public void Equality_SameNestedComponents_EqualAndHaveSameHashCode()
    {
        var left = new PairValue(new object?[] { 1, null }, new TupleValue(2, "three"));
        var right = new PairValue(new object?[] { 1, null }, new TupleValue(2, "three"));

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(right));
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        });
    }

    [Test]
    public void Format_UsesCanonicalPairSyntax()
        => Assert.That(ValueFormatter.Format(new PairValue("BE", 42)), Is.EqualTo("(\"BE\" => 42)"));

    [Test]
    public void Format_RoundTripsWithoutLosingPairType()
    {
        var source = new PairValue("BE", new object?[] { 42m, true });
        var formatted = ValueFormatter.Format(source);
        var parsed = Expression.CreateClosed(formatted).Evaluate(null);

        Assert.That(parsed, Is.InstanceOf<PairValue>().And.EqualTo(source));
    }
}
