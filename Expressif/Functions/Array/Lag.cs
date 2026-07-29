using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the previous value for each input element.
/// The first output value is <see langword="null"/> because there is no previous element.
/// Preserves input cardinality (one output item per input item).
/// Returns <see langword="null"/> when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Lag : BaseArrayFunction
{
    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var output = new List<object?>();
        var hasPrevious = false;
        object? previous = null;

        foreach (var item in enumerable!)
        {
            output.Add(hasPrevious ? previous : null);
            previous = item;
            hasPrevious = true;
        }

        return output.ToArray();
    }
}
