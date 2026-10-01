using System;
using System.Collections;
using Expressif.Functions.Accumulation;
using Expressif.Functions;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Executes an accumulator once over the full input enumerable, then returns
/// the final accumulated value repeated once for each input element.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
[Scope("array/aggregation")]
public class Broadcast : BaseArrayFunction
{
    public Func<IIncrementalAggregation> Aggregation { get; }

    /// <param name="accumulator">Provider for the incremental aggregation used by each broadcast execution.</param>
    public Broadcast(Func<IIncrementalAggregation> accumulator)
        => Aggregation = accumulator;

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
        var session = Aggregation.Invoke().CreateSession();

        var count = 0;
        foreach (var item in enumerable!)
        {
            session.Add(item);
            count++;
        }

        if (count == 0)
            return System.Array.Empty<object?>();

        var finalValue = session.Snapshot();
        var output = new object?[count];
        System.Array.Fill(output, finalValue);
        return output;
    }
}
