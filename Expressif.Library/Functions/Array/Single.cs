using System.Collections;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the only element of the input array without transforming it. Returns <see langword="null"/> when the input is empty, contains more than one element, or cannot be evaluated as an array.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/selection")]
public sealed class Single : BaseArrayFunction<object>
{
    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
                return null;

            var value = enumerator.Current;
            return enumerator.MoveNext() ? null : value;
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}
