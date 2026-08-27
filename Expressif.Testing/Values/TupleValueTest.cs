using Expressif.Values;

namespace Expressif.Testing.Values;

public class TupleValueTest
{
    [Test]
    public void Constructor_EmptyAndSingleField_PreserveArity()
        => Assert.Multiple(() =>
        {
            Assert.That(new Expressif.Values.Tuple(), Has.Count.Zero);
            Assert.That(new Expressif.Values.Tuple(1), Has.Count.EqualTo(1));
        });

    [Test]
    public void Equality_SameNestedFields_EqualAndSameHashCode()
    {
        var left = new TupleValue(1, new TupleValue(2, "three"));
        var right = new TupleValue(1, new TupleValue(2, "three"));

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(right));
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        });
    }

    [Test]
    public void Format_NestedTuple_UsesCanonicalSyntax()
        => Assert.That(ValueFormatter.Format(new TupleValue(1, new TupleValue(2, 3))), Is.EqualTo("T(1, T(2, 3))"));

    [Test]
    public void PublicTuple_IsImmutableCanonicalValueType()
    {
        object actual = new Expressif.Values.Tuple(10, 20);
        object expected = new Expressif.Values.Tuple(10, 20);
        Assert.That(actual, Is.EqualTo(expected));
    }
}
