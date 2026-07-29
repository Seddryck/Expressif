using System;
using System.Collections;
using Expressif.Accumulators;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Executes an accumulator once over the full input enumerable, then returns
/// the final accumulated value repeated once for each input element.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Broadcast : BaseArrayFunction
{
    public Func<IAccumulator> Accumulator { get; }

    /// <param name="accumulator">Factory that creates the accumulator instance used for the broadcast execution.</param>
    public Broadcast(Func<IAccumulator> accumulator)
        => Accumulator = accumulator;

    /// <param name="accumulator">
    /// Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`).
    /// </param>
    public Broadcast(Func<string> accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator.Invoke())) { }

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Broadcast(string accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator)) { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var accumulator = Accumulator.Invoke();
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
