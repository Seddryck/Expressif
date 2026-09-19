using System;
using Expressif.Values.Special;

namespace Expressif.Functions.Temporal;

/// <summary>
/// Returns the shortest unsigned duration between the current time and a reference time on a 24-hour clock. Returns `null` when either time is `null`.
/// </summary>
[Function(prefix: "")]
public sealed class CircularDistance : IFunction<TimeOnly?, TimeSpan?>
{
    private static readonly TimeSpan Day = TimeSpan.FromDays(1);

    public Func<TimeOnly?> Reference { get; }

    /// <param name="reference">The time from which to measure the shortest distance around the clock.</param>
    public CircularDistance(Func<TimeOnly?> reference)
        => Reference = reference;

    public TimeSpan? Evaluate(TimeOnly? value)
    {
        var reference = Reference.Invoke();
        if (value is null || reference is null)
            return null;

        var linear = (value.Value - reference.Value).Duration();
        return linear <= Day - linear ? linear : Day - linear;
    }

    public object? Evaluate(object? value)
        => value switch
        {
            null => null,
            DBNull => null,
            TimeOnly time => Evaluate(time),
            _ when new Null().Equals(value) => null,
            _ => null,
        };
}
