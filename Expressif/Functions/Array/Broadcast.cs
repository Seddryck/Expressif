using Expressif.Functions;
using Expressif.Accumulators;
using System;

namespace Expressif.Functions.Array;

/// <summary>
/// Executes an accumulator once over the full input enumerable, then returns
/// the final accumulated value repeated once for each input element.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Broadcast : IFunction
{
    public Func<string> AccumulatorName { get; }

    /// <param name="accumulator">
    /// Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`).
    /// </param>
    public Broadcast(Func<string> accumulator)
        => AccumulatorName = accumulator;

    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var accumulator = AccumulatorFactory.Instantiate(AccumulatorName.Invoke());
        accumulator.Initialize();

        var count = 0;
        foreach (var item in enumerable!)
        {
            accumulator.Accumulate(item);
            count++;
        }

        if (count == 0)
            return System.Array.Empty<object?>();

        var finalValue = accumulator.GetValue();
        var output = new object?[count];
        System.Array.Fill(output, finalValue);
        return output;
    }
}
