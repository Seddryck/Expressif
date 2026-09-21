using System.Collections.Concurrent;
using Expressif.Bindings;
using Expressif.Observability;
using Expressif.Syntax;

namespace Expressif.Testing.Observability;

[TestFixture]
public class ExpressionObserverTest
{
    [Test]
    public void DefaultObserver_DoesNotChangeEvaluation()
    {
        var expression = new ExpressionFactory(new ExpressionBinder()).Create("upper");

        Assert.That(expression.Evaluate("foo"), Is.EqualTo("FOO"));
    }

    [Test]
    public void ConfiguredObserver_ObservesLifecycleInOrder()
    {
        var observer = new TrackingObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("upper");
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
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("fold(sum)");
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
        var exception = Assert.Catch(() => new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("upper("));
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
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("upper");

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

    [Test]
    public void BeginFailure_DoesNotChangeSuccessfulOperation()
        => AssertSuccessfulOperationIsolatedFrom(ObserverCallback.Begin);

    [Test]
    public void CompleteFailure_DoesNotChangeSuccessfulOperation()
        => AssertSuccessfulOperationIsolatedFrom(ObserverCallback.Complete);

    [Test]
    public void DisposeFailure_DoesNotChangeSuccessfulOperation()
        => AssertSuccessfulOperationIsolatedFrom(ObserverCallback.Dispose);

    [TestCase(ExpressionObservationStage.Parse)]
    [TestCase(ExpressionObservationStage.Bind)]
    [TestCase(ExpressionObservationStage.Evaluate)]
    public void FailureCallbackFailures_PreserveOriginalOperationFailure(ExpressionObservationStage stage)
    {
        var expected = new InvalidOperationException($"{stage} failed");
        var observer = new ThrowingObserver(ObserverCallback.Fail | ObserverCallback.Dispose);

        var actual = Assert.Catch(CreateFailingOperation(stage, observer, expected));

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.SameAs(expected));
            Assert.That(observer.Failure, Is.SameAs(expected));
            Assert.That(
                observer.Events.Where(x => x.EndsWith($":{stage}", StringComparison.Ordinal)),
                Is.EqualTo(new[] { $"begin:{stage}", $"fail:{stage}", $"dispose:{stage}" }));
        });
    }

    private static void AssertSuccessfulOperationIsolatedFrom(ObserverCallback callback)
    {
        var observer = new ThrowingObserver(callback);
        var factory = new ExpressionFactory(
            new StubBinder(new PassThroughExpression()),
            new StubParser(),
            observer);

        var expression = factory.Create("ignored");

        Assert.That(expression.Evaluate("value"), Is.EqualTo("value"));
    }

    private static Action CreateFailingOperation(
        ExpressionObservationStage stage,
        IExpressionObserver observer,
        Exception exception)
        => stage switch
        {
            ExpressionObservationStage.Parse => () => new ExpressionFactory(
                new StubBinder(new PassThroughExpression()),
                new ThrowingParser(exception),
                observer).Create("ignored"),
            ExpressionObservationStage.Bind => () => new ExpressionFactory(
                new ThrowingBinder(exception),
                new StubParser(),
                observer).Create("ignored"),
            ExpressionObservationStage.Evaluate => () => new ExpressionFactory(
                new StubBinder(new ThrowingExpression(exception)),
                new StubParser(),
                observer).Create("ignored").Evaluate("value"),
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null),
        };

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

    [Flags]
    private enum ObserverCallback
    {
        Begin = 1,
        Complete = 2,
        Fail = 4,
        Dispose = 8,
    }

    private sealed class ThrowingObserver(ObserverCallback callbacks) : IExpressionObserver
    {
        public ConcurrentQueue<string> Events { get; } = new();
        public Exception? Failure { get; private set; }

        public IExpressionObservation Begin(ExpressionObservationStage stage)
        {
            Events.Enqueue($"begin:{stage}");
            if (callbacks.HasFlag(ObserverCallback.Begin))
                throw new ObserverException(ObserverCallback.Begin);

            return new ThrowingObservation(this, stage, callbacks);
        }

        private sealed class ThrowingObservation(
            ThrowingObserver owner,
            ExpressionObservationStage stage,
            ObserverCallback callbacks) : IExpressionObservation
        {
            public void Complete()
            {
                owner.Events.Enqueue($"complete:{stage}");
                if (callbacks.HasFlag(ObserverCallback.Complete))
                    throw new ObserverException(ObserverCallback.Complete);
            }

            public void Fail(Exception exception)
            {
                owner.Events.Enqueue($"fail:{stage}");
                owner.Failure = exception;
                if (callbacks.HasFlag(ObserverCallback.Fail))
                    throw new ObserverException(ObserverCallback.Fail);
            }

            public void Dispose()
            {
                owner.Events.Enqueue($"dispose:{stage}");
                if (callbacks.HasFlag(ObserverCallback.Dispose))
                    throw new ObserverException(ObserverCallback.Dispose);
            }
        }
    }

    private sealed class ObserverException(ObserverCallback callback)
        : Exception($"Observer callback {callback} failed.");

    private sealed class StubParser : IExpressionParser
    {
        public RootExpressionSyntax Parse(string text) => ExpressionParser.Parse("upper");
    }

    private sealed class ThrowingParser(Exception exception) : IExpressionParser
    {
        public RootExpressionSyntax Parse(string text) => throw exception;
    }

    private sealed class StubBinder(IExpression expression) : IExpressionBinder
    {
        public IExpression Bind(RootExpressionSyntax syntax) => expression;
        public IExpression BindClosed(RootExpressionSyntax syntax) => expression;
    }

    private sealed class ThrowingBinder(Exception exception) : IExpressionBinder
    {
        public IExpression Bind(RootExpressionSyntax syntax) => throw exception;
        public IExpression BindClosed(RootExpressionSyntax syntax) => throw exception;
    }

    private sealed class PassThroughExpression : IExpression
    {
        public object? Evaluate(object? value) => value;
        public IExpression WithContext(EvaluationContext context) => this;
    }

    private sealed class ThrowingExpression(Exception exception) : IExpression
    {
        public object? Evaluate(object? value) => throw exception;
        public IExpression WithContext(EvaluationContext context) => this;
    }
}
