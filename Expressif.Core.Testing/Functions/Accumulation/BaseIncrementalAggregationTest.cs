using System.Collections;
using Expressif.Functions;
using Expressif.Functions.Accumulation;

namespace Expressif.Testing.Functions.Accumulation;

public sealed class BaseIncrementalAggregationTest
{
    [Test]
    public void EvaluateEnumerableCreatesSessionAndAddsEveryItem()
    {
        var aggregation = new TrackingAggregation();

        var result = aggregation.Evaluate(new object?[] { 1, null, 3 });

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new object?[] { 1, null, 3 }));
            Assert.That(aggregation.CreatedSessions, Is.EqualTo(1));
        });
    }

    [Test]
    public void EvaluateObjectAcceptsEnumerableButRejectsStringAndScalar()
    {
        var aggregation = new TrackingAggregation();

        Assert.Multiple(() =>
        {
            Assert.That(aggregation.Evaluate((object?)new[] { 1, 2 }), Is.EqualTo(new object?[] { 1, 2 }));
            Assert.That(aggregation.Evaluate((object?)"text"), Is.Null);
            Assert.That(aggregation.Evaluate((object?)42), Is.Null);
        });
    }

    [Test]
    public void EnumerableFunctionContractDelegatesToAccumulatorEvaluation()
    {
        IFunction<IEnumerable, object?> aggregation = new TrackingAggregation();

        Assert.That(aggregation.Evaluate(new[] { "a", "b" }), Is.EqualTo(new object?[] { "a", "b" }));
    }

    [Test]
    public void SessionSnapshotIsNonTerminalAndStable()
    {
        var session = new TrackingAggregation().CreateSession();
        session.Add(1);
        var first = session.Snapshot();
        session.Add(2);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(new object?[] { 1 }));
            Assert.That(session.Snapshot(), Is.EqualTo(new object?[] { 1, 2 }));
        });
    }

    [Test]
    public void CreateSessionIsolatesEvaluationState()
    {
        var aggregation = new TrackingAggregation();
        var first = aggregation.CreateSession();
        var second = aggregation.CreateSession();
        first.Add(1);

        Assert.Multiple(() =>
        {
            Assert.That(first.Snapshot(), Is.EqualTo(new object?[] { 1 }));
            Assert.That(second.Snapshot(), Is.Empty);
        });
    }

    private sealed class TrackingAggregation : BaseIncrementalAggregation
    {
        public int CreatedSessions { get; private set; }

        public override IAggregationSession CreateSession()
        {
            CreatedSessions++;
            return new Session();
        }

        private sealed class Session : IAggregationSession
        {
            private readonly List<object?> values = [];

            public void Add(object? item) => values.Add(item);

            public object? Snapshot() => values.ToArray();
        }
    }
}
