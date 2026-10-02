namespace Expressif.Library.Flow;

[Function(prefix: "", aliases: ["conditional-backward"], Name = "conditional-forward")]
internal sealed class ConditionalForward(
    Func<object?, object?> expression,
    Func<object?, object?> predicate,
    bool testCandidate) : IFunction
{
    public object? Evaluate(object? value)
    {
        if (!testCandidate)
        {
            if (ControlFlowBranch.RequireBoolean(predicate.Invoke(value)))
            {
                EvaluationRuntime.ReportFlowDecision(Observability.FlowDecisionOutcome.ExpressionSelected);
                return expression.Invoke(value);
            }

            EvaluationRuntime.ReportFlowDecision(Observability.FlowDecisionOutcome.OriginalInputRetained);
            return value;
        }

        var candidate = expression.Invoke(value);
        if (ControlFlowBranch.RequireBoolean(predicate.Invoke(candidate)))
        {
            EvaluationRuntime.ReportFlowDecision(Observability.FlowDecisionOutcome.CandidateSelected);
            return candidate;
        }

        EvaluationRuntime.ReportFlowDecision(Observability.FlowDecisionOutcome.OriginalInputRetained);
        return value;
    }
}
