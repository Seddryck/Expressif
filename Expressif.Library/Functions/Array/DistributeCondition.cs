using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Predicates;

namespace Expressif.Functions.Array;

/// <summary>
/// Distributes array values into matching and non-matching groups by evaluating a predicate once for each value. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class DistributeCondition : BaseArrayFunction
{
    public Func<IPredicate> Condition { get; }

    /// <param name="condition">Specifies the predicate used to classify each input value.</param>
    public DistributeCondition(Func<IPredicate> condition)
        => Condition = condition;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        if (enumerable is null)
            return null;

        var condition = Condition.Invoke();
        var matching = new List<object?>();
        var nonMatching = new List<object?>();

        foreach (var item in enumerable)
        {
            if (condition.Evaluate(item))
                matching.Add(item);
            else
                nonMatching.Add(item);
        }

        return new object?[][] { matching.ToArray(), nonMatching.ToArray() };
    }
}
