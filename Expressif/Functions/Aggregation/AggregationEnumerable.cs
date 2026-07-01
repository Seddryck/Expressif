using System.Collections;

namespace Expressif.Functions.Array;

internal static class AggregationEnumerable
{
    public static bool TryGetEnumerable(object? value, out IEnumerable? enumerable)
    {
        enumerable = value as IEnumerable;
        if (enumerable is null || value is string)
        {
            enumerable = null;
            return false;
        }

        return true;
    }
}
