using Expressif.Values;

namespace Expressif.Functions.Sorting;

/// <summary>Returns the first count rows and all comparer-equal boundary ties in sort table order.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class TopWithTies : IFunction<SortTableValue, object?[]>
{
    private readonly Func<int> count;

    /// <param name="count">The requested row count before including boundary ties.</param>
    public TopWithTies(Func<int> count)
        => this.count = count;

    public object?[] Evaluate(SortTableValue value)
        => SortSelection.Evaluate(value, count.Invoke(), bottom: false, withTies: true);

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("top-with-ties requires a SortTable value.", nameof(value));
}
