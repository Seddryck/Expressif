using System.Collections.Concurrent;
using Expressif.Bindings;
using Expressif.Observability;

namespace Expressif.Testing.Observability;

[TestFixture]
public class FunctionObserverTest
{
    [Test]
    public void MultipleObservers_ReceiveSuccessfulCallsInRegistrationOrder()
    {
        var events = new ConcurrentQueue<string>();
        var first = new TrackingObserver("first", events);
        var second = new TrackingObserver("second", events);
        var expression = new ExpressionFactory(new ExpressionBinder())
            .WithFunctionObservers([first, second])
            .Create("upper | length");

        var result = expression.Evaluate("abc");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(3));
            Assert.That(events, Is.EqualTo(new[]
            {
                "first:upper:abc:ABC", "second:upper:abc:ABC",
                "first:length:ABC:3", "second:length:ABC:3",
            }));
            Assert.That(first.Completed.Select(item => item.Context.Id), Is.Unique);
        });
    }

    [Test]
    public void FailedFunction_NotifiesObserversAndPreservesException()
    {
        var observer = new TrackingObserver("observer", new());
        var expression = new ExpressionFactory(new ExpressionBinder())
            .WithFunctionObservers([observer])
            .Create("fold(sum)");

        var exception = Assert.Catch(() => expression.Evaluate(new[] { "unknown" }));

        Assert.Multiple(() =>
        {
            Assert.That(observer.Failed, Has.Count.EqualTo(1));
            Assert.That(observer.Failed.Single().Exception, Is.SameAs(exception));
            Assert.That(observer.Failed.Single().Context.Name, Is.EqualTo("fold"));
        });
    }

    [Test]
    public void ObserverFailure_DoesNotChangeFunctionResult()
    {
        var expression = new ExpressionFactory(new ExpressionBinder())
            .WithFunctionObservers([new ThrowingObserver()])
            .Create("upper");

        Assert.That(expression.Evaluate("abc"), Is.EqualTo("ABC"));
    }

    [Test]
    public void NestedCalls_AreObservedWithDistinctNodeIdentities()
    {
        var observer = new TrackingObserver("observer", new());
        var expression = new ExpressionFactory(new ExpressionBinder())
            .WithFunctionObservers([observer])
            .Create("map(upper)");

        var deferred = (System.Collections.IEnumerable)expression.Evaluate(new[] { "a", "b" })!;
        var result = deferred.Cast<object?>().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(observer.Completed.Count(item => item.Context.Name == "upper"), Is.EqualTo(2));
            Assert.That(observer.Completed.Single(item => item.Context.Name == "map").Output, Is.InstanceOf<System.Collections.IEnumerable>());
            Assert.That(observer.Completed.Select(item => item.Context.Id).Distinct().ToArray(), Has.Length.EqualTo(2));
        });
    }

    [Test]
    public void ConcurrentEvaluations_CanShareObserver()
    {
        var observer = new TrackingObserver("observer", new());
        var expression = new ExpressionFactory(new ExpressionBinder())
            .WithFunctionObservers([observer])
            .Create("upper");

        Parallel.ForEach(Enumerable.Range(0, 50), index =>
            Assert.That(expression.Evaluate($"value-{index}"), Is.EqualTo($"VALUE-{index}")));

        Assert.That(observer.Completed, Has.Count.EqualTo(50));
    }

    private sealed class TrackingObserver(string name, ConcurrentQueue<string> events) : IFunctionObserver
    {
        public ConcurrentQueue<(FunctionObservationContext Context, object? Input, object? Output)> Completed { get; } = new();
        public ConcurrentQueue<(FunctionObservationContext Context, object? Input, Exception Exception)> Failed { get; } = new();

        public void OnCompleted(FunctionObservationContext context, object? input, object? output)
        {
            Completed.Enqueue((context, input, output));
            events.Enqueue($"{name}:{context.Name}:{input}:{output}");
        }

        public void OnFailed(FunctionObservationContext context, object? input, Exception exception)
            => Failed.Enqueue((context, input, exception));
    }

    private sealed class ThrowingObserver : IFunctionObserver
    {
        public void OnCompleted(FunctionObservationContext context, object? input, object? output)
            => throw new InvalidOperationException("Observer failed.");

        public void OnFailed(FunctionObservationContext context, object? input, Exception exception)
            => throw new InvalidOperationException("Observer failed.");
    }
}
