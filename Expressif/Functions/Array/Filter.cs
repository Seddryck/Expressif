using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif.Functions.Array;

/// <summary>
/// Applies a predicate expression to each input item and returns only items
/// for which the predicate evaluates to <see langword="true"/>.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function(prefix: "", aliases: ["filter"])]
public class Filter : BaseArrayFunction
{
    public Func<IPredicate> Predicate { get; }

    /// <param name="predicate">Expression defining the predicate applied to each input item.</param>
    public Filter(Func<IPredicate> predicate)
        => Predicate = predicate;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var predicate = Predicate.Invoke();
        var output = new List<object?>();
        foreach (var item in enumerable!)
            if (predicate.Evaluate(item))
                output.Add(item);

        return output.ToArray();
    }
}
