using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

/// <summary>Expands a grouping into all combinations of retained and aggregated dimensions.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class Cube : IFunction<GroupingValue, GroupingValue>
{
    public GroupingValue Evaluate(GroupingValue value)
        => GroupingLevels.Expand(value, SubsetLevels);

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;

    private static IEnumerable<bool[]> SubsetLevels(int dimensions)
    {
        var aggregated = new bool[dimensions];
        while (true)
        {
            yield return [.. aggregated];
            var index = dimensions - 1;
            while (index >= 0 && aggregated[index])
                aggregated[index--] = false;
            if (index < 0)
                yield break;
            aggregated[index] = true;
        }
    }
}
