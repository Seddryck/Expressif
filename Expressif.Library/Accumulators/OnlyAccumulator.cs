using Expressif.Predicates;

namespace Expressif.Accumulators;

/// <summary>
/// Forwards only items satisfying the predicate to the wrapped accumulator.
/// </summary>
[Accumulator(prefix: "", aliases: [])]
public class OnlyAccumulator : BaseAccumulator
{
    private readonly IPredicate predicate;
    private readonly IAccumulator accumulator;

    /// <param name="predicate">Specifies the predicate deciding which items participate.</param>
    /// <param name="accumulator">Specifies the accumulator receiving matching items.</param>
    public OnlyAccumulator(IPredicate predicate, IAccumulator accumulator)
        => (this.predicate, this.accumulator) = (predicate, accumulator);

    public override void Initialize()
        => accumulator.Initialize();

    public override void Accumulate(object? item)
    {
        if (predicate.Evaluate(item))
            accumulator.Accumulate(item);
    }

    public override object? GetValue()
        => accumulator.GetValue();
}
