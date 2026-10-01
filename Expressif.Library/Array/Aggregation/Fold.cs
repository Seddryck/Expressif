using System;
using System.Collections;
using Expressif.Functions.Accumulation;
using Expressif.Functions;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Executes an accumulator once over the full input enumerable and returns
/// the final accumulated value.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
[Function]
[Scope("array/aggregation")]
public class Fold : BaseArrayFunction<object>
{
    public Func<IIncrementalAggregation> Aggregation { get; }

    /// <param name="accumulator">Provider for the incremental aggregation used by each fold execution.</param>
    public Fold([ArgumentRole(ArgumentRole.Accumulator)]
        [ProviderLifetime(ProviderLifetime.FreshPerRequest)] Func<IIncrementalAggregation> accumulator)
        => Aggregation = accumulator;

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Fold(Func<string> accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator.Invoke())) { }

    /// <param name="accumulator">Accumulator name (`count`, `sum`, `min`, `max`, `first`, `last`, ...).</param>
    public Fold(string accumulator)
        : this(() => AccumulatorFactory.Instantiate(accumulator)) { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var session = Aggregation.Invoke().CreateSession();
        foreach (var item in enumerable!)
            session.Add(item);

        return session.Snapshot();
    }
}
