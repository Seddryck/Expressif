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
public class ClosestAccumulator : BaseArrayAggregation
{
    private readonly Func<object?> targetProvider;

    /// <param name="target">Specifies the reference value used to measure numeric or temporal distance.</param>
    public ClosestAccumulator([ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> target)
        => targetProvider = target;

    public override IAggregationSession CreateSession()
    {
        var target = targetProvider.Invoke();
        if (Expressif.Values.Special.Null.Instance.Equals(target))
            return new Session(null);

        IFunction difference = new NumericCaster().TryCast(target!, out var numeric)
            ? new Subtract(() => numeric)
            : new DurationBetween(() => target);
        return new Session(difference);
    }

    private sealed class Session(IFunction? difference) : IAggregationSession
    {
        private decimal? minimumDistance;
        private object? closest;

        public void Add(object? item)
        {
            if (difference is null || Expressif.Values.Special.Null.Instance.Equals(item))
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

        public object? Snapshot() => closest;
    }
}
