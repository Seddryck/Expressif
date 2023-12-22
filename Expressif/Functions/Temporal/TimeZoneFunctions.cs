using Expressif.Values;
using System;
using System.Linq;

namespace Expressif.Functions.Temporal;

public abstract class BaseTimeZoneFunction : BaseTemporalFunction
{
    private static readonly char[] separator = [','];

    protected virtual string[] Tokenize(string label) =>
        label.Replace("(", ",")
        .Replace(")", ",")
        .Replace(":", ",")
        .Replace(" ", string.Empty)
        .Split(separator, StringSplitOptions.RemoveEmptyEntries);
}

public abstract class BaseTimeZoneParameteredFunction : BaseTimeZoneFunction
{
    public Func<string> TimeZoneLabel { get; }

    public BaseTimeZoneParameteredFunction(Func<string> timeZoneLabel)
        => TimeZoneLabel = timeZoneLabel;
    protected TimeZoneInfo InstantiateTimeZoneInfo(string label)
    {
        var zones = TimeZoneInfo.GetSystemTimeZones();
        var zone = zones.SingleOrDefault(z => z.Id == label)
            ?? zones.SingleOrDefault(z => Tokenize(z.DisplayName).Contains(label.Replace(" ", string.Empty)));

        return zone ?? throw new ArgumentOutOfRangeException($"TimeZone '{label}' is not existing on this computer.");
    }

}

/// <summary>
/// Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.
/// </summary>
[Function(prefix: "")]
public class UtcToLocal : BaseTimeZoneParameteredFunction
{
    public UtcToLocal(Func<string> timeZoneLabel)
        : base(timeZoneLabel) { }

    protected override object EvaluateDateTime(DateTime value) =>
        TimeZoneInfo.ConvertTimeFromUtc(value, InstantiateTimeZoneInfo(TimeZoneLabel.Invoke()!));
}

/// <summary>
/// Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.
/// </summary>
[Function(prefix: "")]
public class LocalToUtc : BaseTimeZoneParameteredFunction
{
    public LocalToUtc(Func<string> timeZoneLabel)
        : base(timeZoneLabel)
    { }

    protected override object EvaluateDateTime(DateTime value) =>
        TimeZoneInfo.ConvertTimeToUtc(value, InstantiateTimeZoneInfo(TimeZoneLabel.Invoke()!));
}

/// <summary>
/// Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to UTC
/// </summary>
[Function(prefix: "")]
public class SetToUtc : BaseTimeZoneFunction
{
    protected override object EvaluateDateTime(DateTime value) =>
        new DateTime(value.Ticks, DateTimeKind.Utc);
}

/// <summary>
/// Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to local
/// </summary>
[Function(prefix: "")]
public class SetToLocal : BaseTimeZoneFunction
{
    protected override object EvaluateDateTime(DateTime value) =>
        new DateTime(value.Ticks, DateTimeKind.Local);
}
