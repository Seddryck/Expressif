using System.Collections;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

internal static class GroupingLevels
{
    public static GroupingValue Expand(GroupingValue value, Func<int, IEnumerable<bool[]>> levels)
    {
        if (value.Count == 0)
            return value;

        var tupleKeys = value[0].Key is TupleValue;
        var dimensions = value[0].Key is TupleValue first ? first.Count : 1;
        foreach (var group in value)
        {
            if ((group.Key is TupleValue) != tupleKeys || (group.Key is TupleValue tuple && tuple.Count != dimensions))
                throw new ArgumentException("Grouping keys must be all scalar or all tuples of the same arity.", nameof(value));
            var components = group.Key is TupleValue key ? key.ToArray() : new[] { group.Key };
            if (components.Any(component => component is AllDimension))
                throw new ArgumentException("Grouping keys must not contain an aggregated dimension.", nameof(value));
        }

        var pairs = new List<PairValue>();
        foreach (var aggregated in levels(dimensions))
        {
            // Keep the original level intact, including specialized tuple runtime types.
            if (!aggregated.Any(component => component))
            {
                pairs.AddRange(value);
                continue;
            }

            var buckets = new List<(object? Key, List<object?> Values)>();
            foreach (var group in value)
            {
                object? key = tupleKeys
                    ? new Values.Tuple(((TupleValue)group.Key!).Select((component, index) => aggregated[index] ? AllDimension.Instance : component).ToArray())
                    : AllDimension.Instance;
                var index = buckets.FindIndex(bucket => StructuralComparisons.StructuralEqualityComparer.Equals(bucket.Key, key));
                if (index < 0)
                    buckets.Add((key, group.Values.ToList()));
                else
                    buckets[index].Values.AddRange(group.Values);
            }
            pairs.AddRange(buckets.Select(bucket => new PairValue(bucket.Key, bucket.Values)));
        }
        return new GroupingValue(pairs);
    }
}
