using Expressif.Values;

namespace Expressif.Functions.Sorting;

/// <summary>Returns up to count original values from the first rows in sort table order.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class Top : IFunction<SortTableValue, object?[]>
{
    private readonly Func<int> count;

    /// <param name="count">The maximum number of rows to select.</param>
    public Top(Func<int> count)
        => this.count = count;

    public object?[] Evaluate(SortTableValue value)
        => SortSelection.Evaluate(value, count.Invoke(), bottom: false);

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("top requires a SortTable value.", nameof(value));
}
