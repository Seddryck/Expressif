using System;

namespace Expressif.Functions.Temporal;

/// <summary>
/// Returns the signed duration between the previous temporal value passed as parameter
/// and the current temporal value passed as argument.
/// </summary>
[Function(prefix: "")]
public class DurationBetween : BaseTemporalFunction
{
    public Func<DateTime> Previous { get; }

    /// <param name="previous">The temporal value to subtract from the current argument value.</param>
    public DurationBetween(Func<DateTime> previous)
        => Previous = previous;

    protected override object? EvaluateUncasted(object value)
    {
        try
        {
            return base.EvaluateUncasted(value);
        }
        catch
        {
            return null;
        }
    }

    protected override object EvaluateDateTime(DateTime value)
    {
        try
        {
            return value - Previous.Invoke();
        }
        catch
        {
            return null!;
        }
    }
}
