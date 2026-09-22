using Expressif.Values;

namespace Expressif.Library.Sorting;

/// <summary>Groups original sort table row values by their one-based dense rank without gaps.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class DenseRank : IFunction<SortTableValue, Values.Grouping>
{
    public Values.Grouping Evaluate(SortTableValue value)
        => Rank.EvaluateGroups(value, dense: true);

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("dense-rank requires a SortTable value.", nameof(value));
}
