using System.Collections;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Array.Combination;

using Expressif.Library.Array.Grouping;

public abstract class BaseOuterJoin : BaseArrayFunction<IEnumerable>
{
    private static readonly IEqualityComparer KeyComparer = StructuralComparisons.StructuralEqualityComparer;
    private readonly Func<object?> right;
    private readonly Func<object?, object?> leftKey;
    private readonly Func<object?, object?>? rightKey;
    private readonly bool preserveLeft;
    private readonly bool preserveRight;

    protected BaseOuterJoin(Func<object?> right, Func<object?, object?> leftKey, Func<object?, object?>? rightKey, bool preserveLeft, bool preserveRight)
        => (this.right, this.leftKey, this.rightKey, this.preserveLeft, this.preserveRight) = (right, leftKey, rightKey, preserveLeft, preserveRight);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var source = right.Invoke();
        PairValue[] entries;
        GroupingValue grouping;
        if (source is GroupingValue groups)
        {
            grouping = groups;
            entries = groups.SelectMany(group => group.Values.Select(value => new PairValue(group.Key, value))).ToArray();
        }
        else if (source is DictionaryValue dictionary)
        {
            entries = dictionary.ToArray();
            grouping = GroupingOperations.Group(entries);
        }
        else if (AggregationEnumerable.TryGetEnumerable(source, out var values))
        {
            entries = values!.Cast<object?>().Select(value => new PairValue((rightKey ?? leftKey).Invoke(value), value)).ToArray();
            grouping = GroupingOperations.Group(entries);
        }
        else
        {
            return null;
        }

        var matched = new bool[grouping.Count];
        var result = new List<PairValue>();
        foreach (var left in enumerable)
        {
            var key = leftKey.Invoke(left);
            var index = -1;
            for (var i = 0; i < grouping.Count; i++)
            {
                if (KeyComparer.Equals(grouping[i].Key, key))
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0 && grouping[index].Count > 0)
            {
                matched[index] = true;
                foreach (var value in grouping[index].Values)
                {
                    result.Add(new PairValue(left, value));
                }
            }
            else if (preserveLeft)
            {
                result.Add(new PairValue(left, null));
            }
        }
        if (preserveRight)
        {
            foreach (var entry in entries)
            {
                if (!Enumerable.Range(0, grouping.Count).Any(i => matched[i] && KeyComparer.Equals(grouping[i].Key, entry.Key)))
                {
                    result.Add(new PairValue(null, entry.Value));
                }
            }
        }
        return result.ToArray();
    }
}

/// <summary>Emits every matching pair and preserves unmatched left values with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null.</summary>
[Function(prefix: "")]
[Scope("array/combination")]
public sealed class JoinLeft : BaseOuterJoin
{
    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    public JoinLeft(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey)
        : base(right, leftKey, null, true, false) { }

    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    /// <param name="rightKey">Selects the key of each right array value; when omitted, the left-key expression is reused. It is unnecessary for a grouping or dictionary.</param>
    public JoinLeft(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?>? rightKey = null)
        : base(right, leftKey, rightKey, true, false) { }
}

/// <summary>Emits every matching pair and preserves unmatched right values with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null.</summary>
[Function(prefix: "")]
[Scope("array/combination")]
public sealed class JoinRight : BaseOuterJoin
{
    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    public JoinRight(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey)
        : base(right, leftKey, null, false, true) { }

    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    /// <param name="rightKey">Selects the key of each right array value; when omitted, the left-key expression is reused. It is unnecessary for a grouping or dictionary.</param>
    public JoinRight(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?>? rightKey = null)
        : base(right, leftKey, rightKey, false, true) { }
}

/// <summary>Emits every matching pair and preserves unmatched values from both sides with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null.</summary>
[Function(prefix: "")]
[Scope("array/combination")]
public sealed class JoinFull : BaseOuterJoin
{
    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    public JoinFull(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey)
        : base(right, leftKey, null, true, true) { }

    /// <param name="right">The array, grouping, or dictionary supplying matching right-hand values.</param>
    /// <param name="leftKey">Selects the lookup key of each left value.</param>
    /// <param name="rightKey">Selects the key of each right array value; when omitted, the left-key expression is reused. It is unnecessary for a grouping or dictionary.</param>
    public JoinFull(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?>? rightKey = null)
        : base(right, leftKey, rightKey, true, true) { }
}
