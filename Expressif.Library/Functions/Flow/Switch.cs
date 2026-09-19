namespace Expressif.Functions.Flow;

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
        foreach (var branch in Branches)
        {
            if (branch.Accepts(value))
                return branch.Expression.Invoke(value);
        }
        return null;
    }
}
