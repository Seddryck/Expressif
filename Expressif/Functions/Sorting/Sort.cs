using Expressif.Values;

namespace Expressif.Functions.Sorting;

/// <summary>Stably sorts a normalized sort table and returns its original row values.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class Sort : IFunction<SortTableValue, object?[]>
{
    public object?[] Evaluate(SortTableValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Validate(value);
        return value.Rows.OrderBy(row => row, new RowComparer(value.Headers)).Select(row => row.Value).ToArray();
    }

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("sort requires a SortTable value.", nameof(value));

    private static void Validate(SortTableValue table)
    {
        if (table.Rows.Any(row => row.Keys.Count != table.Headers.Count))
            throw new ArgumentException("Every SortTable row must match the header arity.", nameof(table));
    }

    private sealed class RowComparer(IReadOnlyList<SortHeader> headers) : IComparer<SortRow>
    {
        public int Compare(SortRow? left, SortRow? right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);
            for (var index = 0; index < headers.Count; index++)
            {
                var comparison = CompareValue(headers[index], left.Keys[index], right.Keys[index]);
                if (comparison != 0)
                    return comparison;
            }
            return 0;
        }

        private static int CompareValue(SortHeader header, object? left, object? right)
        {
            if (left is null && right is null)
                return 0;
            if (left is null)
                return header.NullsFirst ? -1 : 1;
            if (right is null)
                return header.NullsFirst ? 1 : -1;

            var ordering = header.Comparer.Compare(left, right)
                ?? throw new InvalidOperationException($"Comparer '{header.Comparer.Name}' returned null for two non-null values.");
            var comparison = ReferenceEquals(ordering, OrderingValue.Less) ? -1
                : ReferenceEquals(ordering, OrderingValue.Greater) ? 1
                : ReferenceEquals(ordering, OrderingValue.Equal) ? 0
                : throw new InvalidOperationException($"Comparer '{header.Comparer.Name}' returned an invalid ordering value.");
            return header.Ascending ? comparison : -comparison;
        }
    }
}
