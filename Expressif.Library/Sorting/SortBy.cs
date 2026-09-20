using System.Collections;
using Expressif.Values;

namespace Expressif.Library.Sorting;

public sealed record SortByCriterion(Func<object?, object?> Evaluate, SortComparer Comparer, bool Ascending, bool NullsFirst);

/// <summary>Stably sorts an array by one or more typed criteria while preserving original elements.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class SortBy : IFunction<IEnumerable, object?[]>
{
    private readonly IReadOnlyList<SortByCriterion> criteria;

    /// <param name="criteria">One or more typed criteria in lexicographic order.</param>
    public SortBy(IEnumerable<SortByCriterion> criteria)
    {
        this.criteria = criteria?.ToArray() ?? throw new ArgumentNullException(nameof(criteria));
        if (this.criteria.Count == 0)
            throw new ArgumentException("sort-by requires at least one criterion.", nameof(criteria));
    }

    public object?[] Evaluate(IEnumerable value)
    {
        return new Sort().Evaluate(BuildTable(value, criteria));
    }

    internal static SortTableValue BuildTable(IEnumerable value, IReadOnlyList<SortByCriterion> criteria)
    {
        var pairs = value.Cast<object?>().Select(item => new Values.Pair(
            new Values.SortKey(criteria.Select(criterion => new Values.SortTerm(
                criterion.Evaluate(item), criterion.Comparer, criterion.Ascending, criterion.NullsFirst)).ToArray()),
            item));
        return (SortTableValue)new SortTable().Evaluate(pairs)!;
    }

    object? IFunction.Evaluate(object? value)
        => value is IEnumerable enumerable && value is not string ? Evaluate(enumerable) : null;
}
