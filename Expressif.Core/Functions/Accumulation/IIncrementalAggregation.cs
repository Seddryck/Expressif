using System.Collections;
using Expressif.Functions;

namespace Expressif.Functions.Accumulation;

/// <summary>
/// Defines an aggregation function that can create isolated sessions for incremental evaluation.
/// </summary>
public interface IIncrementalAggregation : IFunction<IEnumerable, object?>
{
    /// <summary>Creates a new session whose state belongs to one evaluation.</summary>
    IAggregationSession CreateSession();
}
