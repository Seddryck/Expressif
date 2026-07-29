using Expressif.Functions;
using System;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Applies a transformation expression to each input item and returns the transformed values.
/// Preserves input cardinality (one output item per input item).
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function(prefix: "", aliases: ["map"])]
public class Map : IFunction
{
    public Func<IFunction> Transformation { get; }

    public Map(Func<IFunction> transformation)
        => Transformation = transformation;

    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var transformation = Transformation.Invoke();
        var output = new List<object?>();
        foreach (var item in enumerable!)
            output.Add(transformation.Evaluate(item));

        return output.ToArray();
    }
}
