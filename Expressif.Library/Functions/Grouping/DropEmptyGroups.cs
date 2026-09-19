using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using PairValue = Expressif.Values.Pair;

namespace Expressif.Functions.Grouping;

/// <summary>Removes groups whose value collection contains no items.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class DropEmptyGroups : IFunction<GroupingValue, GroupingValue>
{
    public GroupingValue Evaluate(GroupingValue value)
        => new(value
            .Where(group => group.Count > 0)
            .Select(group => new PairValue(group.Key, group.Values)));

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
