using Expressif.Functions;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the next value for each input element.
/// The last output value is <see langword="null"/> because there is no next element.
/// Preserves input cardinality (one output item per input item).
/// Returns <see langword="null"/> when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Lead : IFunction
{
    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var output = new List<object?>();
        var iterator = enumerable!.GetEnumerator();

        if (!iterator.MoveNext())
            return output.ToArray();

        while (iterator.MoveNext())
        {
            output.Add(iterator.Current);
        }

        output.Add(null);
        return output.ToArray();
    }
}
