using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;

namespace Expressif.Functions.Temporal;

[Function(prefix: "dateTime")]
public abstract class BaseTemporalFunction : IFunction
{
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
        if (new Null().Equals(value))
            return EvaluateNull();

        var caster = new DateTimeCaster();
        var dateTime = caster.Cast(value);
        return EvaluateDateTime(dateTime);
    }

    protected virtual object? EvaluateNull() => null;
    protected abstract object EvaluateDateTime(DateTime value);
}

/// <summary>
/// Returns the date at midnight of the argument dateTime.
/// </summary>
[Function(prefix: "", aliases: ["dateTime-to-date"])]
public class DateTimeToDate : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Date;
}

/// <summary>
/// Returns how many years separate the argument dateTime and now.
/// </summary>
[Function(prefix: "", aliases: ["date-to-age"])]
public class Age : BaseTemporalFunction
{
    protected override object EvaluateNull() => 0;
    protected override object EvaluateDateTime(DateTime value)
    {
        // Save today's date.
        var today = DateTime.Today;
        // Calculate the age.
        var age = today.Year - value.Year;
        // Go back to the year the person was born in case of a leap year
        return value.AddYears(age) > today ? age-- : age;
    }
}

/// <summary>
/// Returns the first day of the month of the same month/year than the argument dateTime.
/// </summary>
public class FirstOfMonth : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, value.Month, 1);
}

/// <summary>
/// Returns the first of January of the same year than the argument dateTime.
/// </summary>
public class FirstOfYear : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, 1, 1);
}

/// <summary>
/// Returns the last day of the month of the same month/year than the argument dateTime.
/// </summary>
public class LastOfMonth : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, value.Month, 1).AddMonths(1).AddDays(-1);
}

/// <summary>
/// Returns the 31st of December of the same year than the argument dateTime.
/// </summary>
public class LastOfYear : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, 12, 31);
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
[Function(prefix: "dateTime", aliases:["dateTime-to-clip"])]
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
/// Returns the dateTime argument except if the value is `null` then it returns the parameter value.
/// </summary>
[Function(prefix: "")]
public class NullToDate : BaseTemporalFunction
{
    public Func<DateTime> Default { get; }

    /// <param name="default">The dateTime to be returned if the argument is `null`.</param>
    public NullToDate(Func<DateTime> @default)
        => Default = @default;

    protected override object EvaluateNull() => Default.Invoke();
    protected override object EvaluateDateTime(DateTime value) => value;
}

/// <summary>
/// Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value.
/// </summary>
[Function(prefix: "")]
public class InvalidToDate : BaseTemporalFunction
{
    public Func<DateTime> Default { get; }

    /// <param name="default">The dateTime to be returned if the argument is not a valid dateTime.</param>
    public InvalidToDate(Func<DateTime> @default)
        => Default = @default;

    protected override object EvaluateNull() => new Null();
    protected override object EvaluateDateTime(DateTime value) => value;
    protected override object? EvaluateUncasted(object value)
    {
        if (new Null().Equals(value))
            return EvaluateNull();

        var caster = new DateTimeCaster();
        
        try { return caster.Cast(value);} 
        catch { return Default.Invoke(); }
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
    public Func<string> TimeSpan { get; }

    /// <param name="timeSpan">The value to be added to the argument value</param>
    /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the addition</param>
    public Forward(Func<string> timeSpan, Func<int> times)
        => (TimeSpan, Times) = (timeSpan, times);

    /// <param name="timeSpan">The value to be added to the argument value</param>
    public Forward(Func<string> timeSpan)
        : this(timeSpan, () => 1) { }

    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(System.TimeSpan.Parse(TimeSpan.Invoke()!).Ticks * Times.Invoke());
}

/// <summary>
/// Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.
/// </summary>
[Function(prefix: "dateTime", aliases: ["dateTime-to-subtract"])]
public class Backward : Forward
{
    /// <param name="timeSpan">The value to be subtracted to the argument value.</param>
    /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction</param>
    public Backward(Func<string> timeSpan, Func<int> times)
        : base(timeSpan, times) { }

    /// <param name="timeSpan">The value to be subtracted to the argument value.</param>
    public Backward(Func<string> timeSpan)
        : base(timeSpan) { }

    protected override object EvaluateDateTime(DateTime value)
        => value.AddTicks(System.TimeSpan.Parse(TimeSpan.Invoke()!).Ticks * Times.Invoke() * -1);
}
