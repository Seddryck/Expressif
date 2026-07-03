using System.Linq;
using Expressif.Accumulators;
using Expressif.Functions.Array;

namespace Expressif.Testing.Functions.Array;

public class BroadcastTest
{
    [Test]
    public void Evaluate_CountAccumulator_Valid()
        => Assert.That(new Broadcast(() => new CountAccumulator()).Evaluate(new object?[] { 1, "2", null }), Is.EqualTo(Enumerable.Repeat(3,3)));

    [Test]
    public void Evaluate_SumAccumulator_WithCastableValues_Valid()
        => Assert.That(new Broadcast(() => new SumAccumulator()).Evaluate(new object[] { 1, "2", true }), Is.EqualTo(Enumerable.Repeat(4, 3)));

    [Test]
    public void Evaluate_MinAccumulator_WithCastableValues_Valid()
        => Assert.That(new Broadcast(() => new MinAccumulator()).Evaluate(new object[] { "10", 4, 8.5m }), Is.EqualTo(Enumerable.Repeat(4, 3)));

    [Test]
    public void Evaluate_MaxAccumulator_WithCastableValues_Valid()
        => Assert.That(new Broadcast(() => new MaxAccumulator()).Evaluate(new object[] { "10", 4, 8.5m }), Is.EqualTo(Enumerable.Repeat(10, 3)));

    [Test]
    public void Evaluate_FirstAccumulator_Valid()
        => Assert.That(new Broadcast(() => new FirstAccumulator()).Evaluate(new object[] { 3, 2, 1 }), Is.EqualTo(Enumerable.Repeat(3, 3)));

    [Test]
    public void Evaluate_LastAccumulator_Valid()
        => Assert.That(new Broadcast(() => new LastAccumulator()).Evaluate(new object[] { 3, 2, 1 }), Is.EqualTo(Enumerable.Repeat(1, 3)));

    [Test]
    public void Evaluate_EmptyArray_ExpectedDefaults()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Fold(() => new CountAccumulator()).Evaluate(System.Array.Empty<object>()), Is.EqualTo(0));
            Assert.That(new Fold(() => new SumAccumulator()).Evaluate(System.Array.Empty<object>()), Is.EqualTo(0m));
            Assert.That(new Fold(() => new MinAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new MaxAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new FirstAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new LastAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
        });
    }

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Fold(() => new SumAccumulator()).Evaluate(10), Is.Null);

    [Test]
    public void Evaluate_Broadcast_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 1, 2, 3 });
        var expression = new Expression("broadcast(sum)");

        var result = expression.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { 6m, 6m, 6m }));
        Assert.That(source.GetEnumeratorCalls, Is.EqualTo(1));
    }

    private sealed class SingleEnumerationEnumerable : System.Collections.IEnumerable
    {
        private readonly object?[] values;
        public int GetEnumeratorCalls { get; private set; }

        public SingleEnumerationEnumerable(object?[] values)
            => this.values = values;

        public System.Collections.IEnumerator GetEnumerator()
        {
            GetEnumeratorCalls++;
            if (GetEnumeratorCalls > 1)
                throw new InvalidOperationException("Input was enumerated more than once.");
            return values.GetEnumerator();
        }
    }
}
