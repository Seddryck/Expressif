using Expressif.Functions.Array;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

/// <summary>Keeps up to count complete groups in descending ranking order.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class TopGroups : IFunction<GroupingValue, GroupingValue>
{
    private readonly Func<int> count;
    private readonly Func<IFunction> expression;

    /// <param name="count">The maximum number of groups to select.</param>
    /// <param name="expression">The expression that supplies each group's ranking score.</param>
    public TopGroups(Func<int> count, Func<IFunction> expression)
        => (this.count, this.expression) = (count, expression);

    public GroupingValue Evaluate(GroupingValue value)
    {
        var limit = count.Invoke();
        ArgumentOutOfRangeException.ThrowIfNegative(limit);
        if (limit == 0 || value.Count == 0)
            return new GroupingValue([]);

        var ranking = expression.Invoke();
        var scores = value.Select(group =>
        {
            var score = EvaluationRuntime.EvaluateNested(ranking, group);
            return (Group: group, Score: score is null ? null : ExpressionSelection.Validate(score));
        }).ToArray();
        var comparer = Comparer<object?>.Create((left, right) =>
            left is null ? (right is null ? 0 : -1)
                : right is null ? 1 : ExpressionSelection.Compare(left, right));
        return new GroupingValue(scores.OrderByDescending(item => item.Score, comparer)
            .Take(limit).Select(item => item.Group));
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
