using Expressif.Functions.Array;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

/// <summary>Refines each existing group by appending dimensions derived from its values.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class DrillDown : IFunction<GroupingValue, GroupingValue>
{
    private IReadOnlyList<Func<object?, object?>> Expressions { get; }

    /// <param name="expressions">One or more expressions whose results are appended to the existing key.</param>
    public DrillDown(IEnumerable<Func<object?, object?>> expressions)
        => Expressions = expressions.ToArray();

    public GroupingValue Evaluate(GroupingValue value)
    {
        var pairs = new List<PairValue>();
        foreach (var group in value)
        {
            var prefix = group.Key is TupleValue tuple ? tuple.ToArray() : new[] { group.Key };
            var subgroups = GroupingOperations.Group(group.Values.Select(item =>
                new PairValue(new Values.Tuple([.. prefix, .. Expressions.Select(expression => expression.Invoke(item))]), item)));
            pairs.AddRange(subgroups);
        }
        return new GroupingValue(pairs);
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
