using Expressif.Predicates;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using PairValue = Expressif.Values.Pair;

namespace Expressif.Functions.Grouping;

/// <summary>Keeps whole groups whose group-level predicate evaluates to true.</summary>
[Function(prefix: "", aliases: ["having"])]
[Scope("grouping")]
public sealed class FilterGroups : IFunction<GroupingValue, GroupingValue>
{
    private Func<IPredicate> Predicate { get; }

    /// <param name="predicate">The predicate evaluated once against each group, with its key and value collection available.</param>
    public FilterGroups(Func<IPredicate> predicate)
        => Predicate = predicate;

    public GroupingValue Evaluate(GroupingValue value)
    {
        var predicate = Predicate.Invoke();
        return new GroupingValue(value
            .Where(group =>
            {
                using var scope = EvaluationRuntime.Derive(group);
                return predicate.Evaluate(group);
            })
            .Select(group => new PairValue(group.Key, group.Values)));
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
