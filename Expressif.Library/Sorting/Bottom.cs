using Expressif.Values;

namespace Expressif.Library.Sorting;

/// <summary>Returns up to count original values from the last rows in sort table order.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class Bottom : IFunction<SortTableValue, object?[]>
{
    private readonly Func<int> count;

    /// <param name="count">The maximum number of rows to select.</param>
    public Bottom(Func<int> count)
        => this.count = count;

    public object?[] Evaluate(SortTableValue value)
        => SortSelection.Evaluate(value, count.Invoke(), bottom: true);

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("bottom requires a SortTable value.", nameof(value));
}
