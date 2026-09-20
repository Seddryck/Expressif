using Expressif.Library.Array;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Applies the Library's array coercion rules before starting an accumulator evaluation.
/// </summary>
public abstract class BaseArrayAccumulator : BaseAccumulator
{
    public override object? Evaluate(object? value)
        => AggregationEnumerable.TryGetEnumerable(value, out var enumerable)
            ? Evaluate(enumerable!)
            : null;
}
