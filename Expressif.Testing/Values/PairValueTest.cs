using Expressif.Values;

namespace Expressif.Testing.Values;

public class PairValueTest
{
    [Test]
    public void Equality_WithEquivalentTupleAndGroup_IsSymmetricAndHashCompatible()
    {
        var values = new object?[] { 1, 2 };
        var pair = new PairValue("BE", values);
        var tuple = new TupleValue("BE", values);
        var group = new Group("BE", values);

        Assert.Multiple(() =>
        {
            Assert.That(pair.Equals(tuple), Is.True);
            Assert.That(tuple.Equals(pair), Is.True);
            Assert.That(pair.Equals(group), Is.True);
            Assert.That(group.Equals(tuple), Is.True);
            Assert.That(new[] { pair.GetHashCode(), tuple.GetHashCode(), group.GetHashCode() }, Is.All.EqualTo(pair.GetHashCode()));
        });
    }

    [Test]
    public void Group_CollectionAndTupleViews_RemainDistinct()
    {
        var group = new Group("BE", new object?[] { "Alice", "Bob" });
        var positional = (IPositionalValue)group;

        Assert.Multiple(() =>
        {
            Assert.That(group.ToArray(), Is.EqualTo(new object?[] { "Alice", "Bob" }));
            Assert.That(positional.Arity, Is.EqualTo(2));
            Assert.That(positional.GetPosition(0), Is.EqualTo("BE"));
            Assert.That(positional.GetPosition(1), Is.EqualTo(new object?[] { "Alice", "Bob" }));
        });
    }

    [Test]
    public void Equality_WithEquivalentRecordPosition_IsStructural()
    {
        var pairRecord = new RecordValue();
        pairRecord.Set("code", "BE");
        pairRecord.Set("name", "Bob");
        var tupleRecord = new RecordValue();
        tupleRecord.Set("code", "BE");
        tupleRecord.Set("name", "Bob");

        var pair = new PairValue("BE", pairRecord);
        var tuple = new TupleValue("BE", tupleRecord);

        Assert.Multiple(() =>
        {
            Assert.That(pair.Equals(tuple), Is.True);
            Assert.That(tuple.Equals(pair), Is.True);
            Assert.That(pair.GetHashCode(), Is.EqualTo(tuple.GetHashCode()));
        });
    }

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
