using System.Collections;

namespace Expressif.Functions.Array;

internal static class SpreadValues
{
    public static IEnumerable<object?> Enumerate(object? value)
    {
        if (value is null)
            throw new SpreadArgumentException("Spread argument cannot be null.");

        if (value is string || value is not IEnumerable enumerable)
            throw new SpreadArgumentException("Spread argument must evaluate to an array.");

        foreach (var item in enumerable)
            yield return item;
    }

    public static void Append(object? value, ICollection<object?> target)
    {
        foreach (var item in Enumerate(value))
            target.Add(item);
    }
}
