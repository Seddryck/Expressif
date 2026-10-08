using Expressif.Bindings;
using Expressif.Observability;

namespace Expressif.Testing.Observability;

[TestFixture]
public class ExpressionObserverCompositionTest
{
    [Test]
    public void Combine_EmptyAndSinglePreserveNullAndIdentity()
    {
        var observer = new LifecycleObserver("only", []);

        Assert.Multiple(() =>
        {
            Assert.That(ExpressionObservers.Combine(), Is.Null);
            Assert.That(ExpressionObservers.Combine(observer), Is.SameAs(observer));
        });
    }

    [Test]
    public void Combine_InvokesInRegistrationOrderAndDisposesInReverseOrder()
    {
        var events = new List<string>();
        var observer = ExpressionObservers.Combine(
            new LifecycleObserver("first", events),
            new LifecycleObserver("second", events));

        using (var observation = observer!.Create(ExpressionObservationStage.Evaluate)!)
            observation.Complete();

        Assert.That(events, Is.EqualTo(new[]
        {
            "create:first", "create:second",
            "complete:first", "complete:second",
            "dispose:second", "dispose:first",
        }));
    }

    [Test]
    public void Activate_NestedScopeRestoresOuterFunctionObservation()
    {
        var expression = new ExpressionFactory(new ExpressionBinder()).Create("upper");
        var outer = new FunctionObservation();
        var inner = new FunctionObservation();

        using (outer.Activate())
        {
            expression.Evaluate("first");
            using (inner.Activate())
                expression.Evaluate("inner");
            expression.Evaluate("second");
        }

        Assert.Multiple(() =>
        {
            Assert.That(outer.Completed, Is.EqualTo(2));
            Assert.That(inner.Completed, Is.EqualTo(1));
        });
    }

    [Test]
    public void ObserverReturningNull_DoesNotWrapUnsupportedStages()
    {
        var observer = new UnsupportedObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("upper");

        Assert.Multiple(() =>
        {
            Assert.That(expression.Evaluate("value"), Is.EqualTo("VALUE"));
            Assert.That(observer.Stages, Is.EqualTo(new[]
            {
                ExpressionObservationStage.Parse,
                ExpressionObservationStage.Bind,
                ExpressionObservationStage.Evaluate,
            }));
        });
    }

    private sealed class LifecycleObserver(string name, IList<string> events) : IExpressionObserver
    {
        public IExpressionObservation Create(ExpressionObservationStage stage)
        {
            events.Add($"create:{name}");
            return new Observation(name, events);
        }

        private sealed class Observation(string name, IList<string> events) : IExpressionObservation
        {
            public void Complete() => events.Add($"complete:{name}");
            public void Fail(Exception exception) => events.Add($"fail:{name}");
            public void Dispose() => events.Add($"dispose:{name}");
        }
    }

    private sealed class FunctionObservation : IExpressionObservation, IFunctionObserver
    {
        public int Completed { get; private set; }

        public void OnCompleted(FunctionObservationContext context, object? input, object? output)
            => Completed++;

        public void OnFailed(FunctionObservationContext context, object? input, Exception exception) { }
        public void Complete() { }
        public void Fail(Exception exception) { }
        public void Dispose() { }
    }

    private sealed class UnsupportedObserver : IExpressionObserver
    {
        public List<ExpressionObservationStage> Stages { get; } = [];

        public IExpressionObservation? Create(ExpressionObservationStage stage)
        {
            Stages.Add(stage);
            return null;
        }
    }
}
