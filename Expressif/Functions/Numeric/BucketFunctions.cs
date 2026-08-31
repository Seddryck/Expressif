using System;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Numeric;

[Function(prefix: "")]
[Scope("numeric/classification")]
public abstract class BaseNumericBucketFunction : IFunction<decimal?, int?>
{
    protected BaseNumericBucketFunction(Func<decimal> minimum, Func<decimal> maximum, Func<int> count)
        => (Minimum, Maximum, Count) = (minimum, maximum, count);

    public Func<decimal> Minimum { get; }
    public Func<decimal> Maximum { get; }
    public Func<int> Count { get; }

    public int? Evaluate(decimal? value)
        => value.HasValue ? EvaluateNumeric(value.Value) : null;

    public object? Evaluate(object? value)
    {
        if (value is null || value is DBNull)
            return null;

        if (value is decimal numeric)
            return EvaluateNumeric(numeric);

        if (new Null().Equals(value) || new Empty().Equals(value) || new Whitespace().Equals(value))
            return null;

        return new NumericCaster().TryCast(value, out var cast)
            ? EvaluateNumeric(cast)
            : null;
    }

    protected abstract int? EvaluateOutlier(decimal value, decimal minimum, decimal maximum, int count);

    private int? EvaluateNumeric(decimal value)
    {
        var minimum = Minimum.Invoke();
        var maximum = Maximum.Invoke();
        var count = Count.Invoke();

        if (count <= 0 || maximum <= minimum)
            return null;

        if (value < minimum || value >= maximum)
            return EvaluateOutlier(value, minimum, maximum, count);

        try
        {
            var width = (maximum - minimum) / count;
            if (width == 0)
                return null;

            return decimal.ToInt32(decimal.Floor((value - minimum) / width)) + 1;
        }
        catch (OverflowException)
        {
            return null;
        }
    }
}

/// <summary>
/// Classifies a numeric value into an equal-width bucket within a half-open interval. Returns `null` when the value is outside the interval or the bucket configuration is invalid.
/// </summary>
public class Bucket : BaseNumericBucketFunction
{
    /// <param name="minimum">Inclusive lower bound of the classified interval.</param>
    /// <param name="maximum">Exclusive upper bound of the classified interval.</param>
    /// <param name="count">Strictly positive number of equal-width buckets.</param>
    public Bucket(Func<decimal> minimum, Func<decimal> maximum, Func<int> count)
        : base(minimum, maximum, count)
    { }

    protected override int? EvaluateOutlier(decimal value, decimal minimum, decimal maximum, int count)
        => null;
}

/// <summary>
/// Classifies a numeric value into an equal-width bucket, using additional buckets for values below and above the configured interval. Returns `null` when the bucket configuration is invalid.
/// </summary>
public class BucketWithOutliers : BaseNumericBucketFunction
{
    /// <param name="minimum">Inclusive lower bound of the classified interval.</param>
    /// <param name="maximum">Exclusive upper bound of the classified interval.</param>
    /// <param name="count">Strictly positive number of equal-width in-range buckets.</param>
    public BucketWithOutliers(Func<decimal> minimum, Func<decimal> maximum, Func<int> count)
        : base(minimum, maximum, count)
    { }

    protected override int? EvaluateOutlier(decimal value, decimal minimum, decimal maximum, int count)
        => value < minimum ? 0 : count == int.MaxValue ? null : count + 1;
}
