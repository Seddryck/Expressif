using System.Collections;
using Expressif.Functions;
using Expressif.Functions.Accumulation;

namespace Expressif.Testing.Functions.Accumulation;

public sealed class BaseAccumulatorTest
{
    [Test]
    public void EvaluateEnumerableInitializesAndAccumulatesEveryItem()
    {
        var accumulator = new TrackingAccumulator();

        var result = accumulator.Evaluate(new object?[] { 1, null, 3 });

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new object?[] { 1, null, 3 }));
            Assert.That(accumulator.InitializeCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void EvaluateObjectAcceptsEnumerableButRejectsStringAndScalar()
    {
        var accumulator = new TrackingAccumulator();

        Assert.Multiple(() =>
        {
            Assert.That(accumulator.Evaluate((object?)new[] { 1, 2 }), Is.EqualTo(new object?[] { 1, 2 }));
            Assert.That(accumulator.Evaluate((object?)"text"), Is.Null);
            Assert.That(accumulator.Evaluate((object?)42), Is.Null);
        });
    }

    [Test]
    public void EnumerableFunctionContractDelegatesToAccumulatorEvaluation()
    {
        IFunction<IEnumerable, object?> accumulator = new TrackingAccumulator();

        Assert.That(accumulator.Evaluate(new[] { "a", "b" }), Is.EqualTo(new object?[] { "a", "b" }));
    }

    private sealed class TrackingAccumulator : BaseAccumulator
    {
        private readonly List<object?> values = [];

        public int InitializeCalls { get; private set; }

        public override void Initialize()
        {
            InitializeCalls++;
            values.Clear();
        }

        public override void Accumulate(object? item)
            => values.Add(item);

        public override object? GetValue()
            => values.ToArray();
    }
}
