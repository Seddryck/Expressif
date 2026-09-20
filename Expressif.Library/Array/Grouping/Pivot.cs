using System.Collections;
using Expressif.Bindings;
using Expressif.Values;

namespace Expressif.Library.Array.Grouping;

/// <summary>Groups values by row and column, applies a grouping summary, and reshapes the cells into records.</summary>
[Function(prefix: "")]
[Scope("array/grouping")]
public sealed class Pivot : BaseArrayFunction<RecordValue[]>
{
    private readonly NamedFieldSelector[] row;
    private readonly Func<object?, object?> column;
    private readonly Func<object?, object?> summary;

    /// <param name="row">A direct field selector or tuple of direct field selectors defining row dimensions and their output field names.</param>
    /// <param name="column">An expression defining column keys, which are coerced to output field names.</param>
    /// <param name="summary">An expression transforming the generated grouping into a dictionary with the same composite keys.</param>
    public Pivot(NamedFieldSelector[] row, Func<object?, object?> column, Func<object?, object?> summary)
        => (this.row, this.column, this.summary) = ([.. row], column, summary);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        object? Scalar(object? value)
        {
            if (value is TupleValue)
                throw new ArgumentException("Tuple-valued pivot keys require named row dimensions and are not supported as column keys or individual row fields.");
            return value;
        }

        var keys = row.Select<NamedFieldSelector, Func<object?, object?>>(selector => value => Scalar(selector.Evaluate(value)))
            .Append(value => Scalar(column(value))).ToArray();
        var grouping = (Expressif.Values.Grouping)new GroupBy(keys).Evaluate(enumerable)!;
        if (summary.Invoke(grouping) is not DictionaryValue cells
            || cells.Count != grouping.Count
            || cells.Any(cell => !grouping.Any(group => StructuralComparisons.StructuralEqualityComparer.Equals(group.Key, cell.Key))))
        {
            throw new ArgumentException("The pivot summary must return a dictionary preserving every generated row/column composite key.");
        }

        var nested = new Dictionary.Nest().Evaluate(cells);
        var result = new List<RecordValue>();
        void Visit(DictionaryValue level, List<PairValue> fields)
        {
            foreach (var entry in level)
            {
                var next = new List<PairValue>(fields) { new(row[fields.Count].Name, entry.Key) };
                var inner = (DictionaryValue)entry.Value!;
                if (next.Count == row.Length)
                    result.Add(new Record.FromPairs().Evaluate(next.Concat(inner)));
                else
                    Visit(inner, next);
            }
        }

        Visit(nested, []);
        return result.ToArray();
    }
}
