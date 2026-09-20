using System.Collections;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Array.Combination;

using Expressif.Library.Array.Grouping;

/// <summary>Emits a pair for every matching left and right value, omitting left values without a match. Array right-hand values are grouped by key before lookup.</summary>
[Function(prefix: "")]
[Scope("array/combination")]
public sealed class Join : BaseArrayFunction<IEnumerable>
{
    private static readonly IEqualityComparer KeyComparer = StructuralComparisons.StructuralEqualityComparer;
    private readonly Func<object?> right;
    private readonly Func<object?, object?> leftKey;
    private readonly Func<object?, object?>? rightKey;

    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    public Join(Func<object?> right, Func<object?, object?> leftKey)
        : this(right, leftKey, null) { }

    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    /// <param name="rightKey">Selects the key of each right array value; when omitted, the left-key expression is reused. It is unnecessary for a grouping or dictionary.</param>
    public Join(Func<object?> right, Func<object?, object?> leftKey, Func<object?, object?>? rightKey)
        => (this.right, this.leftKey, this.rightKey) = (right, leftKey, rightKey);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var source = right.Invoke();
        object? keyed = source switch
        {
            GroupingValue grouping => grouping,
            DictionaryValue dictionary => dictionary,
            _ when AggregationEnumerable.TryGetEnumerable(source, out var values)
                => GroupingOperations.Group(values!.Cast<object?>()
                    .Select(value => new PairValue((rightKey ?? leftKey).Invoke(value), value))),
            _ => null,
        };
        if (keyed is null)
            return null;

        var result = new List<PairValue>();
        foreach (var left in enumerable)
        {
            var key = leftKey.Invoke(left);
            if (keyed is GroupingValue grouping)
            {
                var group = grouping.FirstOrDefault(candidate => KeyComparer.Equals(candidate.Key, key));
                if (group is null)
                {
                    continue;
                }
                foreach (var value in group.Values)
                {
                    result.Add(new PairValue(left, value));
                }
            }
            else if (((DictionaryValue)keyed).TryGetValue(key, out var value))
            {
                result.Add(new PairValue(left, value));
            }
        }
        return result.ToArray();
    }
}
