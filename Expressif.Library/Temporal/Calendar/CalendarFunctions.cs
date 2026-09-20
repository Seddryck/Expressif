using System;
using System.Globalization;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using Expressif.Library.Temporal;

namespace Expressif.Library.Temporal.Calendar;

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
