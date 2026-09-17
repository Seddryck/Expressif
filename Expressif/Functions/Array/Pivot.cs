using System.Collections;
using Expressif.Bindings;
using Expressif.Values;

namespace Expressif.Functions.Array;

/// <summary>Groups values by row and column, applies a grouping summary, and reshapes the cells into records.</summary>
[Function(prefix: "")]
[Scope("array/grouping")]
public sealed class Pivot : BaseArrayFunction<RecordValue[]>
{
    private readonly NamedFieldSelector row;
    private readonly Func<object?, object?> column;
    private readonly Func<object?, object?> summary;

    /// <param name="row">A direct field selector defining row keys and the output row field name.</param>
    /// <param name="column">An expression defining column keys, which are coerced to output field names.</param>
    /// <param name="summary">An expression transforming the generated grouping into a dictionary with the same composite keys.</param>
    public Pivot(NamedFieldSelector row, Func<object?, object?> column, Func<object?, object?> summary)
        => (this.row, this.column, this.summary) = (row, column, summary);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var grouping = (Expressif.Values.Grouping)new GroupBy([row.Evaluate, column]).Evaluate(enumerable)!;
        if (summary.Invoke(grouping) is not DictionaryValue cells
            || cells.Count != grouping.Count
            || cells.Any(cell => !grouping.Any(group => StructuralComparisons.StructuralEqualityComparer.Equals(group.Key, cell.Key))))
        {
            throw new ArgumentException("The pivot summary must return a dictionary preserving every generated row/column composite key.");
        }

        var nested = new Dictionary.Nest().Evaluate(cells);
        var result = new List<RecordValue>();
        foreach (var entry in nested)
        {
            var columns = (DictionaryValue)entry.Value!;
            var pairs = new[] { new PairValue(row.Name, entry.Key) }.Concat(columns);
            result.Add(new Record.FromPairs().Evaluate(pairs));
        }
        return result.ToArray();
    }
}
