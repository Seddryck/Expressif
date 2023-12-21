using Expressif.Values;
using System;
using System.Linq;

namespace Expressif.Functions.Temporal;

/// <summary>
/// Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.
/// </summary>
[Function(prefix: "")]
public class UtcToLocal : BaseTemporalFunction
{
    public Func<string> TimeZoneLabel { get; }

    public UtcToLocal(Func<string> timeZoneLabel)
    {
        TimeZoneLabel = timeZoneLabel;
    }

    protected override object EvaluateDateTime(DateTime value) =>
        TimeZoneInfo.ConvertTimeFromUtc(value, InstantiateTimeZoneInfo(TimeZoneLabel.Invoke()!));

    protected TimeZoneInfo InstantiateTimeZoneInfo(string label)
    {
        var zones = TimeZoneInfo.GetSystemTimeZones();
        var zone = zones.SingleOrDefault(z => z.Id == label)
            ?? zones.SingleOrDefault(z => Tokenize(z.DisplayName).Contains(label.Replace(" ", string.Empty)));

        return zone ?? throw new ArgumentOutOfRangeException($"TimeZone '{label}' is not existing on this computer.");
    }

    private string[] Tokenize(string label) =>
        label.Replace("(", ",")
        .Replace(")", ",")
        .Replace(":", ",")
        .Replace(" ", string.Empty)
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
}

/// <summary>
/// Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.
/// </summary>
[Function(prefix: "")]
public class LocalToUtc : UtcToLocal
{
    public LocalToUtc(Func<string> timeZoneLabel)
        : base(timeZoneLabel)
    { }

    protected override object EvaluateDateTime(DateTime value) =>
        TimeZoneInfo.ConvertTimeToUtc(value, InstantiateTimeZoneInfo(TimeZoneLabel.Invoke()!));
}
