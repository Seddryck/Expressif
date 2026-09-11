using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

/// <summary>Expands a grouping into its original level and progressively coarser prefix levels.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class RollUp : IFunction<GroupingValue, GroupingValue>
{
    public GroupingValue Evaluate(GroupingValue value)
        => GroupingLevels.Expand(value, PrefixLevels);

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;

    private static IEnumerable<bool[]> PrefixLevels(int dimensions)
    {
        for (var retained = dimensions; retained >= 0; retained--)
            yield return Enumerable.Range(0, dimensions).Select(index => index >= retained).ToArray();
    }
}
