using System;

namespace Expressif.Functions.Numeric;

/// <summary>
/// Returns the percentage change from the previous numeric value to the current input value. Returns `null` when the input or parameter cannot be evaluated or when the previous value is zero.
/// </summary>
public class PercentChange : BaseNumericFunction
{
    public Func<decimal> Previous { get; }

    /// <param name="previous">Specifies the previous numeric value used as the percentage-change baseline.</param>
    public PercentChange(Func<decimal> previous)
        => Previous = previous;

    protected override object? EvaluateUncasted(object value)
    {
        try
        {
            return base.EvaluateUncasted(value);
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }

    protected override decimal? EvaluateNumeric(decimal current)
    {
        decimal previous;
        try
        {
            previous = Previous.Invoke();
        }
        catch (Exception)
        {
            return null;
        }

        return previous == 0
            ? null
            : ((current - previous) / previous) * 100;
    }
}
