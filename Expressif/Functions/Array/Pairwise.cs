using System.Collections;
using System.Collections.Generic;
using Expressif.Values;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["pairwise"])]
public class Pairwise : BaseArrayFunction
{
    protected override object? EvaluateArray(IEnumerable enumerable)
        => Enumerate(enumerable);

    private static IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable source)
    {
        var enumerator = source.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
                yield break;

            var previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return new Expressif.Values.Tuple(previous, current);
                previous = current;
            }
        }
        finally
        {
            (enumerator as System.IDisposable)?.Dispose();
        }
    }
}
