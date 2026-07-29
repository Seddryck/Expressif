using System;
using System.Collections;
using Expressif.Accumulators;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Executes an accumulator once over the full input enumerable and returns
/// the final accumulated value.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Fold : BaseArrayFunction
{
    public Func<IAccumulator> Accumulator { get; }

    /// <param name="accumulator">Factory that creates the accumulator instance used for the fold execution.</param>
    public Fold(Func<IAccumulator> accumulator)
        => Accumulator = accumulator;

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Fold(Func<string> accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator.Invoke())) { }

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Fold(string accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator)) { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var accumulator = Accumulator.Invoke();
        accumulator.Initialize();
        foreach (var item in enumerable!)
            accumulator.Accumulate(item);

        return accumulator.GetValue();
    }
}
