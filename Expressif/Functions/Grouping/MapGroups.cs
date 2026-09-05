using System.Collections;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using PairValue = Expressif.Values.Pair;

namespace Expressif.Functions.Grouping;

/// <summary>Transforms each group's value collection while preserving its key and position.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class MapGroups : IFunction<GroupingValue, GroupingValue>
{
    private Func<IFunction> Expression { get; }

    /// <param name="expression">The expression evaluated once against each group's value collection.</param>
    public MapGroups(Func<IFunction> expression)
        => Expression = expression;

    public GroupingValue Evaluate(GroupingValue value)
    {
        var expression = Expression.Invoke();
        return new GroupingValue(value.Select(group =>
        {
            using var scope = EvaluationRuntime.Derive(group.Values);
            var result = expression.Evaluate(group.Values);
            if (result is not IEnumerable collection || result is string)
                throw new ArgumentException("The map-groups expression must return a collection.", nameof(value));
            return new PairValue(group.Key, collection);
        }));
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
