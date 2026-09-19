using System.Collections;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Array;

/// <summary>Associates the input value with a key calculated by one or more expressions.</summary>
[Function(prefix: "")]
[Scope("array/grouping")]
public sealed class Key : IFunction<object?, PairValue>
{
    private IReadOnlyList<Func<object?, object?>> Expressions { get; }

    /// <param name="expressions">One or more expressions evaluated against the input; multiple results form a tuple key.</param>
    public Key(IEnumerable<Func<object?, object?>> expressions)
        => Expressions = expressions.ToArray();

    public PairValue Evaluate(object? value)
    {
        var keys = Expressions.Select(expression => expression.Invoke(value)).ToArray();
        var key = keys.Length == 1 ? keys[0] : new Expressif.Values.Tuple(keys);
        return new Expressif.Values.Pair(key, value);
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>Groups pairs by structurally equal keys while preserving first-seen group and value order.</summary>
[Function(prefix: "")]
[Scope("array/grouping")]
public sealed class Group : BaseArrayFunction<GroupingValue>
{
    protected override object? EvaluateArray(IEnumerable enumerable)
        => GroupingOperations.Group(enumerable.Cast<object?>());
}

/// <summary>Groups input values by keys calculated from one or more expressions.</summary>
[Function(prefix: "")]
[Scope("array/grouping")]
public sealed class GroupBy : BaseArrayFunction<GroupingValue>
{
    private IReadOnlyList<Func<object?, object?>> Expressions { get; }

    /// <param name="expressions">One or more expressions evaluated once per input value; multiple results form a tuple key.</param>
    public GroupBy(IEnumerable<Func<object?, object?>> expressions)
        => Expressions = expressions.ToArray();

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var key = new Key(Expressions);
        return GroupingOperations.Group(enumerable.Cast<object?>().Select(key.Evaluate));
    }
}

internal static class GroupingOperations
{
    private static readonly IEqualityComparer Comparer = StructuralComparisons.StructuralEqualityComparer;

    public static GroupingValue Group(IEnumerable<object?> values)
    {
        var buckets = new List<(object? Key, List<object?> Values)>();
        foreach (var value in values)
        {
            if (value is not PairValue pair)
                throw new ArgumentException("Every value to group must be a pair.", nameof(values));

            var index = buckets.FindIndex(bucket => Comparer.Equals(bucket.Key, pair.Key));
            if (index < 0)
                buckets.Add((pair.Key, [pair.Value]));
            else
                buckets[index].Values.Add(pair.Value);
        }

        return new GroupingValue(buckets.Select(bucket => new PairValue(bucket.Key, bucket.Values.ToArray())));
    }
}
