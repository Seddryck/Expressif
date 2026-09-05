using System.Collections;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class ZipTest
{
    [Conformance]
    public void Zip_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new Zip(() => array).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_IsProgressiveAndUsesOneEnumeratorPerInput()
    {
        var left = new TrackingEnumerable(1, 2, 3);
        var right = new TrackingEnumerable("a", "b", "c");
        var result = ((IEnumerable)new Zip(() => right.Cast<object?>().ToArray()).Evaluate(left)!).GetEnumerator();

        Assert.That(left.MoveNextCalls, Is.Zero);
        Assert.That(result.MoveNext(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(left.GetEnumeratorCalls, Is.EqualTo(1));
            Assert.That(left.MoveNextCalls, Is.EqualTo(1));
            Assert.That(result.Current, Is.EqualTo(new TupleValue(1, "a")));
        });
    }

    private sealed class TrackingEnumerable(params object?[] values) : IEnumerable
    {
        public int GetEnumeratorCalls { get; private set; }
        public int MoveNextCalls { get; private set; }

        public IEnumerator GetEnumerator()
        {
            GetEnumeratorCalls++;
            return Enumerate().GetEnumerator();
        }

        private IEnumerable Enumerate()
        {
            foreach (var value in values)
            {
                MoveNextCalls++;
                yield return value;
            }
        }
    }
}

[TestFixture]
public class ZipCycleTest
{
    [Conformance]
    public void ZipCycle_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new ZipCycle(() => array).Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class ZipPaddedTest
{
    [Conformance]
    public void ZipPadded_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new ZipPadded(() => array).Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class ZipStrictTest
{
    [Conformance]
    public void ZipStrict_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new ZipStrict(() => array).Evaluate(value), Is.EqualTo(expected));

    [TestCase(1, 2)]
    [TestCase(2, 1)]
    public void Evaluate_UnequalLengths_ReturnsNull(int leftLength, int rightLength)
        => Assert.That(
            new ZipStrict(() => new object?[rightLength]).Evaluate(new object?[leftLength]),
            Is.Null);
}
