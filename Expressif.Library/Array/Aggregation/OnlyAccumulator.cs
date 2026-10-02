using Expressif.Predicates;
using Expressif.Functions;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Forwards only items satisfying the predicate to the wrapped incremental aggregation.
/// </summary>
[Function(prefix: "", Name = "only")]
public class OnlyAccumulator : BaseArrayAggregation
{
    private readonly Func<IPredicate> predicateProvider;
    private readonly Func<IIncrementalAggregation> accumulatorProvider;

    /// <param name="predicate">Specifies the predicate deciding which items participate.</param>
    /// <param name="accumulator">Specifies the incremental aggregation receiving matching items.</param>
    public OnlyAccumulator(IPredicate predicate, IIncrementalAggregation accumulator)
        : this(() => predicate, () => accumulator) { }

    internal OnlyAccumulator(
        [ArgumentRole(ArgumentRole.Predicate, AllowValueExpression = true)]
        [ProviderLifetime(ProviderLifetime.FreshPerRequest)]
        Func<IPredicate> predicate,
        [ArgumentRole(ArgumentRole.Accumulator)]
        [ProviderLifetime(ProviderLifetime.FreshPerRequest)]
        Func<IIncrementalAggregation> accumulator)
        => (predicateProvider, accumulatorProvider) = (predicate, accumulator);

    public override IAggregationSession CreateSession()
    {
        var predicate = predicateProvider.Invoke();
        var accumulator = accumulatorProvider.Invoke().CreateSession();
        return new Session(predicate, accumulator);
    }

    private sealed class Session(IPredicate predicate, IAggregationSession accumulator) : IAggregationSession
    {
        public void Add(object? item)
        {
            if (predicate.Evaluate(item))
                accumulator.Add(item);
        }

        public object? Snapshot() => accumulator.Snapshot();
    }
}
