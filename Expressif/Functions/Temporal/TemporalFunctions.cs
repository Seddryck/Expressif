using System;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Temporal;

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
        if (new Null().Equals(value))
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
/// Returns the date at midnight of the argument dateTime.
/// </summary>
[Function(prefix: "", aliases: ["dateTime-to-date"])]
[Scope("temporal/conversion")]
public class DateTimeToDate : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Date;
}

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
/// Returns the date of the Catholic calendar event passed as parameter for the year specified by the argument.
/// Returns `null` if the event is unknown.
/// </summary>
[Function(prefix: "", aliases: ["calendar-catholic"])]
[Scope("temporal/calendar")]
public class CatholicCalendar : BaseDatePartChangeFunction
{
    public Func<string> Event { get; }
    public Func<string> Kind { get; }

    public CatholicCalendar(Func<string> @event)
        : this(@event, () => nameof(DateTimeKind.Local)) { }

    public CatholicCalendar(Func<string> @event, Func<string> kind)
        => (Event, Kind) = (@event, kind);

    protected override object? EvaluateInteger(int numeric) => EvaluateYear(numeric);
    protected override object EvaluateDateTime(DateTime value) => EvaluateYear(value.Year)!;
    protected override object? EvaluateYearMonth(YearMonth yearMonth) => EvaluateYear(yearMonth.Year);

    private DateTime? EvaluateYear(int year)
    {
        var kindValue = Kind.Invoke();
        var kind = InstantiateKind(kindValue);
        var easter = Easter(year, kind);
        return Normalize(Event.Invoke()) switch
        {
            "epiphany" => CreateDate(year, 1, 6, kind),
            "candlemas" => CreateDate(year, 2, 2, kind),
            "annunciation" => CreateDate(year, 3, 25, kind),
            "shrove tuesday" => easter.AddDays(-47),
            "ash wednesday" => easter.AddDays(-46),
            "palm sunday" => easter.AddDays(-7),
            "maundy thursday" => easter.AddDays(-3),
            "good friday" => easter.AddDays(-2),
            "easter sunday" => easter,
            "ascension day" => easter.AddDays(39),
            "pentecost" or "whit sunday" => easter.AddDays(49),
            "whit monday" => easter.AddDays(50),
            "trinity sunday" => easter.AddDays(56),
            "corpus christi" => easter.AddDays(60),
            "assumption" => CreateDate(year, 8, 15, kind),
            "immaculate conception" => CreateDate(year, 12, 8, kind),
            "all saints' day" => CreateDate(year, 11, 1, kind),
            "first sunday of advent" => FirstSundayOfAdvent(year, kind),
            "christmas" => CreateDate(year, 12, 25, kind),
            _ => null,
        };
    }

    private static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().Replace('\u2019', '\'').ToLowerInvariant();
        return normalized.StartsWith("the ") ? normalized[4..].TrimStart() : normalized;
    }

    private static DateTimeKind InstantiateKind(string? kind)
    {
        if (Enum.TryParse<DateTimeKind>(kind, true, out var dateTimeKind))
            return dateTimeKind;
        throw new ArgumentOutOfRangeException(nameof(kind), $"DateTimeKind '{kind}' is not valid.");
    }

    private static DateTime FirstSundayOfAdvent(int year, DateTimeKind kind)
    {
        var december3rd = CreateDate(year, 12, 3, kind);
        return december3rd.AddDays(-(int)december3rd.DayOfWeek);
    }

    private static DateTime Easter(int year, DateTimeKind kind)
    {
        int a = year % 19;
        int b = year / 100;
        int c = (b - (b / 4) - (((8 * b) + 13) / 25) + (19 * a) + 15) % 30;
        int d = c - ((c / 28) * (1 - ((c / 28) * (29 / (c + 1)) * ((21 - a) / 11))));
        int e = d - ((year + (year / 4) + d + 2 - b + (b / 4)) % 7);
        int month = 3 + ((e + 40) / 44);
        int day = e + 28 - (31 * (month / 4));
        return CreateDate(year, month, day, kind);
    }

    private static DateTime CreateDate(int year, int month, int day, DateTimeKind kind)
        => new(year, month, day, 0, 0, 0, kind);
}

/// <summary>
/// Returns the first day of the month of the same month/year than the argument dateTime.
/// </summary>
[Scope("temporal/calendar")]
public class FirstOfMonth : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, value.Month, 1);
}

/// <summary>
/// Returns the first of January of the same year than the argument dateTime.
/// </summary>
[Scope("temporal/calendar")]
public class FirstOfYear : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, 1, 1);
}

/// <summary>
/// Returns the last day of the month of the same month/year than the argument dateTime.
/// </summary>
[Scope("temporal/calendar")]
public class LastOfMonth : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, value.Month, 1).AddMonths(1).AddDays(-1);
}

/// <summary>
/// Returns the 31st of December of the same year than the argument dateTime.
/// </summary>
[Scope("temporal/calendar")]
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
/// Returns the dateTime argument except if the value is `null` then it returns the parameter value.
/// </summary>
[Function(prefix: "")]
[Scope("temporal/conversion")]
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
[Scope("temporal/conversion")]
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

        try { return caster.Cast(value); }
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
