using Expressif.Values;

namespace Expressif.Functions.Sorting;

internal static class SortSelection
{
    private sealed record IndexedRow(SortRow Row, int Position);

    internal static object?[] Evaluate(SortTableValue table, int count, bool bottom, bool withTies = false)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        SortRowComparer.Validate(table);
        if (count == 0 || table.Rows.Count == 0)
            return [];

        var semantic = new SortRowComparer(table.Headers);
        var stable = Comparer<IndexedRow>.Create((left, right) =>
        {
            var result = semantic.Compare(left.Row, right.Row);
            return result != 0 ? result : left.Position.CompareTo(right.Position);
        });
        var priority = bottom ? stable : Comparer<IndexedRow>.Create((left, right) => stable.Compare(right, left));
        var selected = new PriorityQueue<IndexedRow, IndexedRow>(priority);
        var limit = Math.Min(count, table.Rows.Count);
        for (var index = 0; index < table.Rows.Count; index++)
        {
            var row = new IndexedRow(table.Rows[index], index);
            if (selected.Count < limit)
                selected.Enqueue(row, row);
            else if ((bottom ? stable.Compare(row, selected.Peek()) > 0 : stable.Compare(row, selected.Peek()) < 0))
                selected.DequeueEnqueue(row, row);
        }

        var rows = selected.UnorderedItems.Select(item => item.Element).ToList();
        if (withTies)
        {
            var boundary = selected.Peek().Row;
            var positions = rows.Select(row => row.Position).ToHashSet();
            for (var index = 0; index < table.Rows.Count; index++)
            {
                if (!positions.Contains(index) && semantic.Compare(table.Rows[index], boundary) == 0)
                    rows.Add(new IndexedRow(table.Rows[index], index));
            }
        }

        return rows
            .OrderBy(row => row, stable).Select(row => row.Row.Value).ToArray();
    }
}
