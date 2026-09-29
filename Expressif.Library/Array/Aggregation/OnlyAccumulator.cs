using Expressif.Predicates;
using Expressif.Functions;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Forwards only items satisfying the predicate to the wrapped accumulator.
/// </summary>
[Function(prefix: "", Name = "only")]
public class OnlyAccumulator : BaseArrayAccumulator
{
    private readonly Func<IPredicate> predicateProvider;
    private readonly Func<IAccumulator> accumulatorProvider;
    private IPredicate? predicate;
    private IAccumulator? accumulator;

    /// <param name="predicate">Specifies the predicate deciding which items participate.</param>
    /// <param name="accumulator">Specifies the accumulator receiving matching items.</param>
    public OnlyAccumulator(IPredicate predicate, IAccumulator accumulator)
        : this(() => predicate, () => accumulator) { }

    internal OnlyAccumulator(Func<IPredicate> predicate, Func<IAccumulator> accumulator)
        => (predicateProvider, accumulatorProvider) = (predicate, accumulator);

    public override void Initialize()
    {
        predicate = predicateProvider.Invoke();
        accumulator = accumulatorProvider.Invoke();
        accumulator.Initialize();
    }

    public override void Accumulate(object? item)
    {
        if (predicate!.Evaluate(item))
            accumulator!.Accumulate(item);
    }

    public override object? GetValue()
        => accumulator!.GetValue();
}
