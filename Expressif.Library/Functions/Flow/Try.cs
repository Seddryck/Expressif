namespace Expressif.Functions.Flow;

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
        foreach (var branch in Branches)
        {
            var candidate = branch.Expression.Invoke(value);
            if (branch.Accepts(candidate))
                return candidate;
        }
        return null;
    }
}
