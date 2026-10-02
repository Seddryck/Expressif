using System.Collections.Concurrent;
using Expressif.Bindings;
using Expressif.Observability;

namespace Expressif.Testing.Flow;

[TestFixture]
public class FlowObserverTest
{
    [TestCase("catch(42)", "value", "catch", FlowDecisionOutcome.PassThrough, -1, 0, false)]
    [TestCase("is-negative ?> absolute", -12, "conditional-forward", FlowDecisionOutcome.ExpressionSelected, -1, 0, false)]
    [TestCase("is-negative ?> absolute", 12, "conditional-forward", FlowDecisionOutcome.OriginalInputRetained, -1, 0, false)]
    [TestCase("absolute <? greater-than(10)", -12, "conditional-backward", FlowDecisionOutcome.CandidateSelected, -1, 0, false)]
    [TestCase("absolute <? greater-than(10)", -5, "conditional-backward", FlowDecisionOutcome.OriginalInputRetained, -1, 0, false)]
    [TestCase("coalesce(#null, 2)", 1, "coalesce", FlowDecisionOutcome.CandidateSelected, 1, 2, false)]
    [TestCase("switch(#true => 1, _ => 2)", 1, "switch", FlowDecisionOutcome.BranchSelected, 0, 1, false)]
    [TestCase("switch(#false => 1, _ => 2)", 1, "switch", FlowDecisionOutcome.BranchSelected, 1, 2, true)]
    [TestCase("try(absolute => greater-than(10), _ => 2)", -5, "try", FlowDecisionOutcome.CandidateSelected, 1, 2, true)]
    [TestCase("guard(trim)", 42, "guard", FlowDecisionOutcome.IncompatibleInputRetained, -1, 0, false)]
    [TestCase("guard(trim)", " value ", "guard", FlowDecisionOutcome.GuardedExpressionSelected, -1, 0, false)]
    [TestCase("throw(#false)", 1, "throw", FlowDecisionOutcome.InputAccepted, -1, 0, false)]
    public void Evaluation_ReportsSemanticDecision(
        string source,
        object input,
        string function,
        FlowDecisionOutcome outcome,
        int index,
        int evaluated,
        bool isFallback)
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create(source);

        expression.Evaluate(input);

        var item = observer.Decisions.Single(decision => decision.Context.Name == function);
        Assert.Multiple(() =>
        {
            Assert.That(item.Decision.Outcome, Is.EqualTo(outcome));
            Assert.That(item.Decision.Index, Is.EqualTo(index));
            Assert.That(item.Decision.Evaluated, Is.EqualTo(evaluated));
            Assert.That(item.Decision.IsFallback, Is.EqualTo(isFallback));
        });
    }

    [Test]
    public void NullCatch_ReportsRecovery()
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("#null | catch(42)");

        expression.Evaluate(null);

        Assert.That(observer.Decisions.Single().Decision.Outcome, Is.EqualTo(FlowDecisionOutcome.Recovery));
    }

    [TestCase("coalesce(#null, #null)", FlowDecisionOutcome.AllCandidatesNull)]
    [TestCase("switch(#false => 1)", FlowDecisionOutcome.NoBranchMatched)]
    [TestCase("try(1 => #false, 2 => #false)", FlowDecisionOutcome.NoCandidateAccepted)]
    public void ExhaustedFlow_ReportsTerminalOutcome(string source, FlowDecisionOutcome outcome)
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create(source);

        expression.Evaluate(1);

        Assert.That(observer.Decisions.Single().Decision.Outcome, Is.EqualTo(outcome));
    }

    [TestCase("apply(upper)", "value")]
    [TestCase("transform-with(upper, identity)", "value")]
    [TestCase("transform-as(upper, value := identity)", "value")]
    public void FixedPathFunction_DoesNotReportFlowDecision(string source, object input)
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create(source);

        expression.Evaluate(input);
        Assert.That(observer.Decisions, Is.Empty);
    }

    [Test]
    public void RejectedThrow_ReportsBeforeRaisingException()
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("throw(#true)");

        Assert.Throws<Library.Flow.EvaluationException>(() => expression.Evaluate(1));
        Assert.That(observer.Decisions.Single().Decision.Outcome, Is.EqualTo(FlowDecisionOutcome.InputRejected));
    }

    [Test]
    public void SelectedRecovery_IsReportedWhenRecoveryFails()
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer)
            .Create("#null | catch(throw(#true))");

        Assert.Throws<Library.Flow.EvaluationException>(() => expression.Evaluate(null));
        Assert.That(
            observer.Decisions.Any(item => item.Context.Name == "catch"
                && item.Decision.Outcome == FlowDecisionOutcome.Recovery),
            Is.True);
    }

    [Test]
    public void RepeatedFlowNodes_HaveDistinctIdentities()
    {
        var observer = new FlowObserver();
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer)
            .Create("catch(1) | catch(2)");

        expression.Evaluate("value");

        Assert.That(observer.Decisions.Select(item => item.Context.Id).ToArray(), Is.Unique);
    }

    [Test]
    public void ObserverFailure_DoesNotChangeFlowResult()
    {
        var expression = new ExpressionFactory(new ExpressionBinder(), observer: new ThrowingFlowObserver())
            .Create("switch(#true => 1, _ => 2)");

        Assert.That(expression.Evaluate(0), Is.EqualTo(1));
    }

    [Test]
    public void DisabledFlowObservation_DoesNotAllocatePerDecision()
    {
        var flow = new Library.Flow.Switch([new(value => value, null)]);
        const string input = "value";
        for (var index = 0; index < 100; index++)
            flow.Evaluate(input);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10_000; index++)
            flow.Evaluate(input);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.That(allocated, Is.Zero);
    }

    private sealed class FlowObserver : IExpressionObserver
    {
        public ConcurrentQueue<(FunctionObservationContext Context, FlowDecision Decision)> Decisions { get; } = new();

        public IExpressionObservation? Create(ExpressionObservationStage stage)
            => stage == ExpressionObservationStage.Evaluate ? new Observation(this) : null;

        private sealed class Observation(FlowObserver owner) : IExpressionObservation, IFlowObserver
        {
            public void OnDecision(FunctionObservationContext function, FlowDecision decision)
                => owner.Decisions.Enqueue((function, decision));

            public void Complete() { }
            public void Fail(Exception exception) { }
            public void Dispose() { }
        }
    }

    private sealed class ThrowingFlowObserver : IExpressionObserver
    {
        public IExpressionObservation? Create(ExpressionObservationStage stage)
            => stage == ExpressionObservationStage.Evaluate ? new Observation() : null;

        private sealed class Observation : IExpressionObservation, IFlowObserver
        {
            public void OnDecision(FunctionObservationContext function, FlowDecision decision)
                => throw new InvalidOperationException("Observer failed.");

            public void Complete() { }
            public void Fail(Exception exception) { }
            public void Dispose() { }
        }
    }
}
