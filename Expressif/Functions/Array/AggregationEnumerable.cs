using Expressif.Values.Casters;
using System.Collections;

namespace Expressif.Functions.Array;

internal static class AggregationEnumerable
{
    private static readonly ArrayCaster ArrayCaster = new();

    public static bool TryGetEnumerable(object? value, out IEnumerable? enumerable)
    {
        if (value is string text)
            return ArrayCaster.TryParse(text, out var array)
                ? (enumerable = array) == array
                : (enumerable = null) != null;

        enumerable = value as IEnumerable;
        if (enumerable is null)
        {
            enumerable = null;
            return false;
        }

        return true;
    }
}
