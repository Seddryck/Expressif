using System.Collections;
using Expressif.Values;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns a tuple containing the input array's elements in order. Returns `null` when the input is not an array.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array")]
public sealed class ToTuple : BaseArrayFunction<TupleValue>
{
    protected override object EvaluateArray(IEnumerable enumerable)
        => new Values.Tuple(enumerable.Cast<object?>().ToArray());
}
