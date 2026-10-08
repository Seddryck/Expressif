namespace Expressif.Library.Flow;

/// <summary>Returns the first candidate result accepted by its predicate, or the final fallback, or null.</summary>
[Function(prefix: "", DynamicReason = "Output depends on the expression selected by ordered predicate acceptance.")]
[Scope("flow")]
public sealed class Try : IFunction
{
    private IReadOnlyList<ControlFlowBranch> Branches { get; }

    /// <param name="branches">Ordered branches with an optional final catch-all fallback.</param>
    public Try(IEnumerable<ControlFlowBranch> branches)
        => Branches = branches.ToArray();

    public object? Evaluate(object? value)
    {
        for (var index = 0; index < Branches.Count; index++)
        {
            var branch = Branches[index];
            var candidate = branch.Expression.Invoke(value);
            if (branch.Accepts(candidate))
            {
                EvaluationRuntime.ReportFlowDecision(
                    Observability.FlowDecisionOutcome.CandidateSelected,
                    index,
                    index + 1,
                    branch.Predicate is null);
                return candidate;
            }
        }
        EvaluationRuntime.ReportFlowDecision(
            Observability.FlowDecisionOutcome.NoCandidateAccepted,
            evaluated: Branches.Count);
        return null;
    }
}
