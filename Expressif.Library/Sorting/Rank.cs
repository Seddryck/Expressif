using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Sorting;

/// <summary>Groups original sort table row values by their one-based SQL rank.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class Rank : IFunction<SortTableValue, GroupingValue>
{
    public GroupingValue Evaluate(SortTableValue value)
        => EvaluateGroups(value, dense: false);

    internal static GroupingValue EvaluateGroups(SortTableValue value, bool dense)
    {
        ArgumentNullException.ThrowIfNull(value);
        SortRowComparer.Validate(value);
        var comparer = new SortRowComparer(value.Headers);
        var rows = value.Rows.OrderBy(row => row, comparer).ToArray();
        var groups = new List<PairValue>();
        var values = new List<object?>();
        var rank = 1;
        for (var index = 0; index < rows.Length; index++)
        {
            if (index > 0 && comparer.Compare(rows[index], rows[index - 1]) != 0)
            {
                groups.Add(new PairValue(rank, values.ToArray()));
                values.Clear();
                rank = dense ? rank + 1 : index + 1;
            }
            values.Add(rows[index].Value);
        }
        if (rows.Length > 0)
            groups.Add(new PairValue(rank, values.ToArray()));
        return new GroupingValue(groups);
    }

    object? IFunction.Evaluate(object? value)
        => value is SortTableValue table
            ? Evaluate(table)
            : throw new ArgumentException("rank requires a SortTable value.", nameof(value));
}
