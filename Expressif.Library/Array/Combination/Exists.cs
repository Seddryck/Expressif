using System.Collections;
using Expressif.Functions;
using Expressif.Library.Array;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Array.Combination;

/// <summary>Returns whether the input value has a matching key in the supplied array or grouping.</summary>
[Predicate(appendIs: false, prefix: "", name: "exists")]
[Scope("array/combination")]
public sealed class Exists : BasePredicate, IFunction<object, bool>
{
    private static readonly IEqualityComparer KeyComparer = StructuralComparisons.StructuralEqualityComparer;
    private readonly Func<object?> right;
    private readonly Func<object?, object?> leftKey;
    private readonly Func<object?, object?>? rightKey;

    /// <param name="right">The array or grouping supplying matching keys.</param>
    /// <param name="leftKey">Selects the lookup key of the input value.</param>
    public Exists(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey)
        : this(right, leftKey, null) { }

    /// <param name="right">The array or grouping supplying matching keys.</param>
    /// <param name="leftKey">Selects the lookup key of the input value.</param>
    /// <param name="rightKey">Selects each right array value’s key; when omitted, the left-key expression is reused. It is skipped for a grouping.</param>
    public Exists(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> right,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> leftKey,
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] [ArgumentOmission(ArgumentOmissionMode.Absent)] Func<object?, object?>? rightKey = null)
        => (this.right, this.leftKey, this.rightKey) = (right, leftKey, rightKey);

    public override bool Evaluate(object? value)
    {
        var source = right.Invoke();
        var key = leftKey.Invoke(value);
        if (source is GroupingValue grouping)
            return grouping.Any(group => KeyComparer.Equals(group.Key, key));
        if (!AggregationEnumerable.TryGetEnumerable(source, out var values))
            return false;

        foreach (var item in values!)
        {
            if (KeyComparer.Equals(key, (rightKey ?? leftKey).Invoke(item)))
                return true;
        }
        return false;
    }
}
