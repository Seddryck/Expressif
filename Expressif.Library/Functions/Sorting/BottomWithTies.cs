using Expressif.Values;

namespace Expressif.Functions.Sorting;

/// <summary>Returns the last count rows and all comparer-equal boundary ties in sort table order.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class BottomWithTies : IFunction<SortTableValue, object?[]>
{
    private readonly Func<int> count;

    /// <param name="count">The requested row count before including boundary ties.</param>
    public BottomWithTies(Func<int> count)
        => this.count = count;

    public object?[] Evaluate(SortTableValue value)
        => SortSelection.Evaluate(value, count.Invoke(), bottom: true, withTies: true);

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("bottom-with-ties requires a SortTable value.", nameof(value));
}
