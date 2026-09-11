using System.Collections;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

/// <summary>Derives keys from existing grouping keys and merges matching groups into one grouping level.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class DrillUp : IFunction<GroupingValue, GroupingValue>
{
    private Func<object?, object?> Expression { get; }

    /// <param name="expression">The expression that derives a new key from each existing group key.</param>
    public DrillUp(Func<object?, object?> expression)
        => Expression = expression;

    public GroupingValue Evaluate(GroupingValue value)
    {
        var buckets = new List<(object? Key, List<object?> Values)>();
        foreach (var group in value)
        {
            var key = Expression.Invoke(group.Key);
            var index = buckets.FindIndex(bucket => StructuralComparisons.StructuralEqualityComparer.Equals(bucket.Key, key));
            if (index < 0)
                buckets.Add((key, group.Values.ToList()));
            else
                buckets[index].Values.AddRange(group.Values);
        }
        return new GroupingValue(buckets.Select(bucket => new PairValue(bucket.Key, bucket.Values)));
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
