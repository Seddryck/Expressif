using Expressif.Functions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function(prefix: "", aliases: ["reverse"])]
public class Reverse : BaseArrayFunction
{
    public Reverse() { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var buffer = new List<object?>();
        buffer.AddRange(enumerable.Cast<object?>());

        buffer.Reverse();
        return buffer.ToArray();
    }
}
