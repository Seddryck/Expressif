using Expressif.Functions;
using Expressif.Predicates;
using System;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Applies a predicate expression to each input item and returns only items
/// for which the predicate evaluates to <see langword="true"/>.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function(prefix: "", aliases: ["filter"])]
public class Filter : IFunction
{
    public Func<IPredicate> Predicate { get; }

    public Filter(Func<IPredicate> predicate)
        => Predicate = predicate;

    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var predicate = Predicate.Invoke();
        var output = new List<object?>();
        foreach (var item in enumerable!)
            if (predicate.Evaluate(item))
                output.Add(item);

        return output.ToArray();
    }
}
