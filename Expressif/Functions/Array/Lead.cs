using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the next value for each input element.
/// The last output value is <see langword="null"/> because there is no next element.
/// Preserves input cardinality (one output item per input item).
/// Returns <see langword="null"/> when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Lead : BaseArrayFunction
{
    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var output = new List<object?>();
        var hasItems = false;

        foreach (var item in enumerable!)
        {
            if (hasItems)
                output.Add(item);

            hasItems = true;
        }

        if (hasItems)
            output.Add(null);

        return output.ToArray();
    }
}
