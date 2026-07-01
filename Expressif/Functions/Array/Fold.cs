using Expressif.Functions;
using Expressif.Accumulators;
using System;

namespace Expressif.Functions.Array;


/// <summary>
/// Executes an accumulator once over the full input enumerable and returns
/// the final accumulated value.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
public class Fold : IFunction
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

    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var accumulator = Accumulator.Invoke();
        accumulator.Initialize();
        foreach (var item in enumerable!)
            accumulator.Accumulate(item);

        return accumulator.GetValue();
    }
}
