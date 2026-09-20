using Expressif.Values;

namespace Expressif.Library.Sorting;

/// <summary>Stably sorts a normalized sort table and returns its original row values.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class Sort : IFunction<SortTableValue, object?[]>
{
    public object?[] Evaluate(SortTableValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        SortRowComparer.Validate(value);
        return value.Rows.OrderBy(row => row, new SortRowComparer(value.Headers)).Select(row => row.Value).ToArray();
    }

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("sort requires a SortTable value.", nameof(value));
}
