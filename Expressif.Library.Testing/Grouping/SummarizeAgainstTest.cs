using Expressif.Functions.Accumulation;
using Expressif.Functions;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using PairValue = Expressif.Values.Pair;
using TupleValue = Expressif.Values.Tuple;
using SummarizeAgainstFunction = Expressif.Library.Grouping.SummarizeAgainst;

namespace Expressif.Testing.Grouping;

public class SummarizeAgainstTest
{
    [Test]
    public void Evaluate_EmptyGroupingFinalizesGlobalStateWithoutCombining()
    {
        var events = new List<string>();
        var function = new SummarizeAgainstFunction(
            () => new TrackingAccumulator("local", events),
            () => new TrackingAccumulator("global", events),
            () => new TupleOperation(events));

        var result = function.Evaluate(new GroupingValue([]));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(events, Is.EqualTo(new[] { "global.initialize", "global.finalize" }));
        });
    }

    [Test]
    public void Evaluate_UpdatesLocalAndGlobalStatesInOneTraversal()
    {
        var events = new List<string>();
        var nextLocal = 0;
        var function = new SummarizeAgainstFunction(
            () => new TrackingAccumulator($"local{nextLocal++}", events),
            () => new TrackingAccumulator("global", events),
            () => new TupleOperation(events));
        var grouping = new GroupingValue([
            new PairValue("FR", new object?[] { 2, null }),
            new PairValue("BE", new object?[] { 3 }),
        ]);

        var result = function.Evaluate(grouping);

        Assert.Multiple(() =>
        {
            Assert.That(result.Select(pair => pair.Key), Is.EqualTo(new[] { "FR", "BE" }));
            Assert.That(result.Select(pair => pair.Value), Is.EqualTo(new object?[] { 5, 4 }));
            Assert.That(events, Is.EqualTo(new[]
            {
                "global.initialize", "local0.initialize",
                "local0.accumulate:2", "global.accumulate:2",
                "local0.accumulate:null", "global.accumulate:null",
                "local1.initialize", "local1.accumulate:3", "global.accumulate:3",
                "global.finalize", "local0.finalize", "combine", "local1.finalize", "combine",
            }));
        });
    }

    [Test]
    public void Evaluate_PreservesAccumulatorAndCombineFailures()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                () => TestExpression.Create("#{(\"BE\" => {#null})} | summarize-against(sum, count, first)").Evaluate(null),
                Throws.TypeOf<InvalidCastException>());
            var failingCombine = new SummarizeAgainstFunction(
                () => new CountAccumulator(),
                () => new CountAccumulator(),
                () => new ThrowingOperation());
            Assert.That(
                () => failingCombine.Evaluate(new GroupingValue([new PairValue("BE", new object?[] { 1 })])),
                Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("combine failed"));
        });
    }

    private sealed class TrackingAccumulator(string name, List<string> events) : BaseAccumulator
    {
        private int count;

        public override void Initialize()
        {
            events.Add($"{name}.initialize");
            count = 0;
        }

        public override void Accumulate(object? item)
        {
            events.Add($"{name}.accumulate:{item ?? "null"}");
            count++;
        }

        public override object GetValue()
        {
            events.Add($"{name}.finalize");
            return count;
        }
    }

    private sealed class TupleOperation(List<string> events) : IFunction
    {
        public object? Evaluate(object? value)
        {
            events.Add("combine");
            var tuple = (TupleValue)value!;
            return (int)tuple[0]! + (int)tuple[1]!;
        }
    }

    private sealed class ThrowingOperation : IFunction
    {
        public object? Evaluate(object? value) => throw new InvalidOperationException("combine failed");
    }
}
