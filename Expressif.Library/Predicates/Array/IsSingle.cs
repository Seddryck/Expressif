using System.Collections;
using Expressif.Functions;
using Expressif.Functions.Array;

namespace Expressif.Predicates.Array;

/// <summary>
/// Returns whether the input array contains exactly one element. Returns false when the input cannot be evaluated as an array.
/// </summary>
[Predicate(appendIs: false, prefix: "", name: "is-single")]
[Scope("array")]
public sealed class IsSingle : BasePredicate, IPredicate<IEnumerable>, IFunction<IEnumerable, bool>
{
    public override bool Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return false;

        var enumerator = enumerable!.GetEnumerator();
        try
        {
            return enumerator.MoveNext() && !enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    public bool Evaluate(IEnumerable? value) => Evaluate((object?)value);
}
