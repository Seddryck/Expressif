namespace Expressif.Library.Flow;

/// <summary>Returns the result of the first branch whose predicate accepts the original input, or the final fallback, or null.</summary>
[Function(prefix: "", DynamicReason = "Output depends on the expression selected by ordered predicate acceptance.")]
[Scope("flow")]
public sealed class Switch : IFunction
{
    private IReadOnlyList<ControlFlowBranch> Branches { get; }

    /// <param name="branches">Ordered branches with an optional final catch-all fallback.</param>
    public Switch(IEnumerable<ControlFlowBranch> branches)
        => Branches = branches.ToArray();

    public object? Evaluate(object? value)
    {
        for (var index = 0; index < Branches.Count; index++)
        {
            var branch = Branches[index];
            if (branch.Accepts(value))
            {
                EvaluationRuntime.ReportFlowDecision(
                    Observability.FlowDecisionOutcome.BranchSelected,
                    index,
                    index + 1,
                    branch.Predicate is null);
                return branch.Expression.Invoke(value);
            }
        }
        EvaluationRuntime.ReportFlowDecision(
            Observability.FlowDecisionOutcome.NoBranchMatched,
            evaluated: Branches.Count);
        return null;
    }
}
