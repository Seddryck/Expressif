using System.Collections;

namespace Expressif.Functions.Sorting;

/// <summary>Groups array elements by their one-based SQL rank using typed criteria.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class RankBy : IFunction<IEnumerable, Values.Grouping>
{
    private readonly IReadOnlyList<SortByCriterion> criteria;

    /// <param name="criteria">Criteria in the form expression -&gt; :type, optionally followed by direction and null-placement modifiers.</param>
    public RankBy(IEnumerable<SortByCriterion> criteria)
    {
        this.criteria = criteria?.ToArray() ?? throw new ArgumentNullException(nameof(criteria));
        if (this.criteria.Count == 0)
            throw new ArgumentException("Ranking requires at least one criterion.", nameof(criteria));
    }

    public Values.Grouping Evaluate(IEnumerable value)
        => new Rank().Evaluate(SortBy.BuildTable(value, criteria));

    object? IFunction.Evaluate(object? value)
        => value is IEnumerable enumerable && value is not string ? Evaluate(enumerable) : null;
}
