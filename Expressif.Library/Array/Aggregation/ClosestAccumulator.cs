using System;
using Expressif.Functions;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Library.Temporal;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using Expressif.Library.Numeric.Arithmetic;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Returns the first non-null input value with the smallest absolute distance to the target.
/// </summary>
[Function(prefix: "", Name = "closest")]
public class ClosestAccumulator : BaseArrayAccumulator
{
    private readonly Func<object?> targetProvider;
    private IFunction? difference;
    private decimal? minimumDistance;
    private object? closest;

    /// <param name="target">Specifies the reference value used to measure numeric or temporal distance.</param>
    public ClosestAccumulator(Func<object?> target)
        => targetProvider = target;

    public override void Initialize()
    {
        closest = null;
        minimumDistance = null;
        difference = null;
        var target = targetProvider.Invoke();
        if (new Expressif.Values.Special.Null().Equals(target))
            return;

        difference = new NumericCaster().TryCast(target!, out var numeric)
            ? new Subtract(() => numeric)
            : new DurationBetween(() => target);
    }

    public override void Accumulate(object? item)
    {
        if (difference is null || new Expressif.Values.Special.Null().Equals(item))
            return;

        decimal? distance = difference.Evaluate(item) switch
        {
            decimal numeric => Math.Abs(numeric),
            TimeSpan duration => Math.Abs((decimal)duration.Ticks),
            _ => null,
        };
        if (distance.HasValue && (!minimumDistance.HasValue || distance.Value < minimumDistance.Value))
        {
            minimumDistance = distance;
            closest = item;
        }
    }

    public override object? GetValue()
        => closest;
}
