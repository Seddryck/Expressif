using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions.Accumulation;
using Expressif.Functions;

namespace Expressif.Library.Array.Aggregation;

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
    public Func<IIncrementalAggregation> Aggregation { get; }

    /// <param name="accumulator">Provider for the incremental aggregation used by each scan execution.</param>
    public Scan([ArgumentRole(ArgumentRole.Accumulator)] [ProviderLifetime(ProviderLifetime.FreshPerRequest)] Func<IIncrementalAggregation> accumulator)
        => Aggregation = accumulator;

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Scan(Func<string> accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator.Invoke())) { }

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Scan(string accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator)) { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var session = Aggregation.Invoke().CreateSession();

        var output = new List<object?>();
        foreach (var item in enumerable!)
        {
            session.Add(item);
            output.Add(session.Snapshot());
        }

        return output.ToArray();
    }
}
