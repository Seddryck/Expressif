using System;
using System.Globalization;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Library.Temporal;

[Function(prefix: "dateTime")]
public abstract class BaseTemporalFunction<TOut> : IFunction<DateTime?, TOut?>
{
    TOut? IFunction<DateTime?, TOut?>.Evaluate(DateTime? value)
        => Evaluate((object?)value) is TOut result ? result : default;

    public object? Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull _ => EvaluateNull(),
            DateOnly date => EvaluateDateTime(date.ToDateTime(TimeOnly.MinValue)),
            DateTime dt => EvaluateDateTime(dt),
            DateTimeOffset dto => EvaluateDateTime(dto.UtcDateTime),
            _ => EvaluateUncasted(value),
        };
    }

    protected virtual object? EvaluateUncasted(object value)
    {
        if (new Expressif.Values.Special.Null().Equals(value))
            return EvaluateNull();

        return new DateTimeCaster().TryCast(value, out var dateTime)
            ? EvaluateDateTime(dateTime)
            : EvaluateNull();
    }

    protected virtual object? EvaluateNull() => null;
    protected abstract object EvaluateDateTime(DateTime value);
}

public abstract class BaseTemporalFunction : BaseTemporalFunction<DateTime?>
{ }

/// <summary>
/// Returns the completed years between the argument dateTime and the current date. Returns `null` for null or future dates. In a non-leap year, a February 29 birthday is reached on February 28.
/// </summary>
[Function(prefix: "", aliases: ["date-to-age"])]
public class Age : BaseTemporalFunction<int?>
{
    protected override object EvaluateDateTime(DateTime value)
    {
        var today = GetCurrentDate();
        if (value.Date > today)
            return null!;

        var age = today.Year - value.Year;
        return value.AddYears(age).Date > today ? age - 1 : age;
    }

    private static DateTime GetCurrentDate()
        => EvaluationRuntime.Context is { } context
            && context.TryGetVariable("current-date", out var value)
            && value is not null
                ? new DateTimeCaster().Cast(value).Date
                : DateTime.Today;
}

/// <summary>
/// Returns the day immediately following the dateTime passed as argument value.
/// </summary>
public class NextDay : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.AddDays(1);
}

/// <summary>
/// Returns the dateTime that adds a month to the dateTime passed as argument value.
/// </summary>
public class NextMonth : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.AddMonths(1);
}

/// <summary>
/// Returns the dateTime that adds a year to the dateTime passed as argument value.
/// </summary>
public class NextYear : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.AddYears(1);
}

/// <summary>
/// Returns the dateTime that substract a day to the dateTime passed as argument value.
/// </summary>
public class PreviousDay : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.AddDays(-1);
}

/// <summary>
/// Returns the dateTime that substract a month to the dateTime passed as argument value.
/// </summary>
public class PreviousMonth : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.AddMonths(-1);
}

/// <summary>
/// Returns the dateTime that substract a year to the dateTime passed as argument value.
/// </summary>
public class PreviousYear : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.AddYears(-1);
}

/// <summary>
/// Returns the value of an argument dateTime, unless it is before min (in which case it returns min), or after max (in which case it returns max).
/// </summary>
[Function(prefix: "dateTime", aliases: ["dateTime-to-clip"])]
public class Clamp : BaseTemporalFunction
{
    public Func<DateTime> Min { get; }
    public Func<DateTime> Max { get; }

    /// <param name="min">value returned in case the argument value is before than it</param>
    /// <param name="max">value returned in case the argument value is after than it</param>
    public Clamp(Func<DateTime> min, Func<DateTime> max)
        => (Min, Max) = (min, max);

    protected override object EvaluateDateTime(DateTime value)
        => (value < Min.Invoke()) ? Min.Invoke() : (value > Max.Invoke()) ? Max.Invoke() : value;
}

/// <summary>
/// Returns the signed duration between the current temporal value and a previous temporal value. Returns `null` when either value cannot be evaluated or the temporal values are incompatible.
/// </summary>
[Function(prefix: "")]
public class DurationBetween : BaseTemporalFunction<TimeSpan?>
{
    public Func<object?> Previous { get; }

    /// <param name="previous">The previous temporal value to subtract from the current input.</param>
    public DurationBetween(Func<object?> previous)
        => Previous = previous;

    protected override object? EvaluateUncasted(object value)
        => new DateTimeCaster().TryCast(value, out var dateTime)
            ? EvaluateDateTime(dateTime)
            : null;

    protected override object EvaluateDateTime(DateTime value)
    {
        var previous = Previous.Invoke();
        return previous is not null && new DateTimeCaster().TryCast(previous, out var dateTime)
            ? value - dateTime
            : null!;
    }
}

/// <summary>
/// Returns a dateTime with the time part set to the value passed as parameter and the date part corresponding to the argument value.
/// </summary>
public class SetTime : BaseTemporalFunction
{
    public Func<string> Instant { get; }

    /// <param name="instant">The time value to set as hours, minutes, seconds of the dateTime argument</param>
    public SetTime(Func<string> instant)
        => Instant = instant;

    protected override object EvaluateDateTime(DateTime value)
    {
        var time = TimeSpan.Parse(Instant.Invoke()!);
        return new DateTime(value.Year, value.Month, value.Day, time.Hours, time.Minutes, time.Seconds);
    }
}

/// <summary>
/// Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero.
/// </summary>
public class FloorHour : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(-1 * (value.Ticks % TimeSpan.TicksPerHour));
}

/// <summary>
/// Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero and an hour added.
/// </summary>
public class CeilingHour : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(TimeSpan.TicksPerHour - (value.Ticks % TimeSpan.TicksPerHour == 0 ? TimeSpan.TicksPerHour : value.Ticks % TimeSpan.TicksPerHour));
}

/// <summary>
/// Returns the dateTime passed as argument value with the seconds and milliseconds set to zero.
/// </summary>
public class FloorMinute : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(-1 * (value.Ticks % TimeSpan.TicksPerMinute));
}

/// <summary>
/// Returns the dateTime passed as argument value with the seconds and milliseconds set to zero and a minute added.
/// </summary>
public class CeilingMinute : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(TimeSpan.TicksPerMinute - (value.Ticks % TimeSpan.TicksPerMinute == 0 ? TimeSpan.TicksPerMinute : value.Ticks % TimeSpan.TicksPerMinute));
}

/// <summary>
/// Returns a dateTime that adds the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.
/// </summary>
[Function(prefix: "dateTime", aliases: ["dateTime-to-add"])]
public class Forward : BaseTemporalFunction
{
    public Func<int> Times { get; }
    public Func<TimeOnly> Time { get; }

    /// <param name="time">The value to be added to the argument value</param>
    /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the addition</param>
    public Forward(Func<TimeOnly> time, Func<int> times)
        => (Time, Times) = (time, times);

    /// <param name="time">The value to be added to the argument value</param>
    public Forward(Func<TimeOnly> time)
        : this(time, () => 1) { }

    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(Time.Invoke().ToTimeSpan().Ticks * Times.Invoke());
}

/// <summary>
/// Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.
/// </summary>
[Function(prefix: "dateTime", aliases: ["dateTime-to-subtract"])]
public class Backward : Forward
{
    /// <param name="time">The value to be subtracted to the argument value.</param>
    /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction</param>
    public Backward(Func<TimeOnly> time, Func<int> times)
        : base(time, times) { }

    /// <param name="time">The value to be subtracted to the argument value.</param>
    public Backward(Func<TimeOnly> time)
        : base(time) { }

    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(Time.Invoke().ToTimeSpan().Ticks * Times.Invoke() * -1);
}
