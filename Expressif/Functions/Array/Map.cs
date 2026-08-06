using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Applies a transformation expression to each input item and returns the transformed values.
/// Preserves input cardinality (one output item per input item).
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function(prefix: "", aliases: ["map"])]
public class Map : BaseArrayFunction
{
    public Func<IFunction> Transformation { get; }

    /// <param name="transformation">Factory that creates the transformation applied to each input item.</param>
    public Map(Func<IFunction> transformation)
        => Transformation = transformation;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var transformation = Transformation.Invoke();
        var output = new List<object?>();
        foreach (var item in enumerable!)
            output.Add(transformation.Evaluate(item));

        return output.ToArray();
    }
}
