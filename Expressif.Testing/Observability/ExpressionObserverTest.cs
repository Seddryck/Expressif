using System.Collections.Concurrent;
using Expressif.Observability;

namespace Expressif.Testing.Observability;

[TestFixture]
public class ExpressionObserverTest
{
    [Test]
    public void DefaultObserver_DoesNotChangeEvaluation()
    {
        var expression = new ExpressionFactory().Create("upper");

        Assert.That(expression.Evaluate("foo"), Is.EqualTo("FOO"));
    }

    [Test]
    public void ConfiguredObserver_ObservesLifecycleInOrder()
    {
        var observer = new TrackingObserver();
        var expression = new ExpressionFactory(observer: observer).Create("upper");
        var result = expression.Evaluate("foo");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("FOO"));
            Assert.That(observer.Observations, Has.All.Matches<TrackingObservation>(x => x.IsComplete && x.Failure is null));
            Assert.That(observer.Events, Is.EqualTo(new[]
            {
                "begin:Parse", "end:Parse",
                "begin:Bind", "end:Bind",
                "begin:Evaluate", "end:Evaluate",
            }));
        });
    }

    [Test]
    public void FailedEvaluation_ReportsOriginalFailureAndDisposesScope()
    {
        var observer = new TrackingObserver();
        var expression = new ExpressionFactory(observer: observer).Create("fold(sum)");
        var exception = Assert.Catch(() => expression.Evaluate(new[] { "unknown" }));
        var evaluation = observer.Observations.Single(x => x.Stage == ExpressionObservationStage.Evaluate);
        Assert.Multiple(() =>
        {
            Assert.That(evaluation.Failure, Is.SameAs(exception));
            Assert.That(evaluation.IsComplete, Is.False);
            Assert.That(evaluation.IsDisposed, Is.True);
        });
    }

    [Test]
    public void FailedParsing_ReportsFailureAndDisposesScope()
    {
        var observer = new TrackingObserver();
        var exception = Assert.Catch(() => new ExpressionFactory(observer: observer).Create("upper("));
        var parsing = observer.Observations.Single();
        Assert.Multiple(() =>
        {
            Assert.That(parsing.Stage, Is.EqualTo(ExpressionObservationStage.Parse));
            Assert.That(parsing.Failure, Is.SameAs(exception));
            Assert.That(parsing.IsComplete, Is.False);
            Assert.That(parsing.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Evaluate_ConcurrentCallsUseDistinctObservations()
    {
        var observer = new TrackingObserver();
        var expression = new ExpressionFactory(observer: observer).Create("upper");

        Parallel.ForEach(Enumerable.Range(0, 50), i =>
            Assert.That(expression.Evaluate($"value-{i}"), Is.EqualTo($"VALUE-{i}")));

        var evaluations = observer.Observations
            .Where(x => x.Stage == ExpressionObservationStage.Evaluate)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(evaluations, Has.Length.EqualTo(50));
            Assert.That(evaluations.Select(x => x.Id), Is.Unique);
            Assert.That(evaluations, Has.All.Matches<TrackingObservation>(x => x.IsDisposed));
        });
    }

    private sealed class TrackingObserver : IExpressionObserver
    {
        private int nextId;

        public ConcurrentQueue<string> Events { get; } = new();
        public ConcurrentQueue<TrackingObservation> Observations { get; } = new();

        public IExpressionObservation Begin(ExpressionObservationStage stage)
        {
            Events.Enqueue($"begin:{stage}");
            var observation = new TrackingObservation(
                Interlocked.Increment(ref nextId),
                stage,
                () => Events.Enqueue($"end:{stage}"));
            Observations.Enqueue(observation);
            return observation;
        }
    }

    private sealed class TrackingObservation(
        int id,
        ExpressionObservationStage stage,
        Action onDispose) : IExpressionObservation
    {
        private int disposed;

        public int Id { get; } = id;
        public ExpressionObservationStage Stage { get; } = stage;
        public bool IsDisposed => disposed != 0;
        public bool IsComplete { get; private set; }
        public Exception? Failure { get; private set; }

        public void Complete() => IsComplete = true;

        public void Fail(Exception exception) => Failure = exception;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
                onDispose();
        }
    }
}
