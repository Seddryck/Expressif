using System.Collections;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

public class PairwiseTest
{
    [Conformance]
    public void Pairwise_Valid(object? value, object? expected)
        => Assert.That(new Pairwise().Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_Values_ReturnsConsecutiveTuples()
        => Assert.That(new Pairwise().Evaluate(new object?[] { 100, 105, 120 }),
            Is.EqualTo(new[] { new TupleValue(100, 105), new TupleValue(105, 120) }));

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(2, 1)]
    public void Evaluate_Boundaries_CorrectCardinality(int count, int expected)
        => Assert.That(((IEnumerable)new Pairwise().Evaluate(Enumerable.Range(0, count).ToArray())!).Cast<object>().Count(), Is.EqualTo(expected));

    [Test]
    public void Evaluate_Nulls_ArePreserved()
        => Assert.That(new Pairwise().Evaluate(new object?[] { 100, null, 120 }),
            Is.EqualTo(new[] { new TupleValue(100, null), new TupleValue(null, 120) }));

    [Test]
    public void Evaluate_IsProgressiveAndUsesOneEnumerator()
    {
        var source = new TrackingEnumerable(1, 2, 3);
        var result = ((IEnumerable)new Pairwise().Evaluate(source)!).GetEnumerator();

        Assert.That(source.MoveNextCalls, Is.Zero);
        Assert.That(result.MoveNext(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(source.GetEnumeratorCalls, Is.EqualTo(1));
            Assert.That(source.MoveNextCalls, Is.EqualTo(2));
            Assert.That(result.Current, Is.EqualTo(new TupleValue(1, 2)));
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
