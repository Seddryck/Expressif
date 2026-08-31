using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Accumulators;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Executes an accumulator progressively over the input enumerable and returns
/// the intermediate accumulated value after each input element.
/// Preserves input cardinality (one output item per input item).
/// This differs from fold (final value only) and broadcast (final value repeated).
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
[Scope("array/aggregation")]
public class Scan : BaseArrayFunction
{
    public Func<IAccumulator> Accumulator { get; }

    /// <param name="accumulator">Factory that creates the accumulator instance used for the scan execution.</param>
    public Scan(Func<IAccumulator> accumulator)
        => Accumulator = accumulator;

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Scan(Func<string> accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator.Invoke())) { }

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Scan(string accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator)) { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var accumulator = Accumulator.Invoke();
        accumulator.Initialize();

        var output = new List<object?>();
        foreach (var item in enumerable!)
        {
            accumulator.Accumulate(item);
            output.Add(accumulator.GetValue());
        }

        return output.ToArray();
    }
}
