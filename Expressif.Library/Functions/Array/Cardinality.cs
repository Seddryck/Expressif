using System.Collections;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the number of elements in the input array.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array")]
public sealed class Cardinality : BaseArrayFunction<int>
{
    protected override object EvaluateArray(IEnumerable enumerable)
        => enumerable.Cast<object?>().Count();
}
