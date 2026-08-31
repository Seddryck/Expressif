using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Expressif.Functions.Temporal;
using Expressif.Functions.Text;
using Expressif.Testing.Conformance;
using Expressif.Values.Special;
using NUnit.Framework;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class TemporalFunctionsTest
{
    [Conformance]
    public void DurationBetween_Valid_Previous(object input, object previous, TimeSpan? expected)
        => Assert.That(new DurationBetween(() => previous).Evaluate(input), Is.EqualTo(expected));

    [Conformance]
    public void Clamp_Valid(object value, DateTime min, DateTime max, DateTime expected)
        => Assert.That(new Clamp(() => min, () => max)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NextDay_Valid(object value, DateTime expected)
        => Assert.That(new NextDay().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NextMonth_Valid(object value, DateTime expected)
        => Assert.That(new NextMonth().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NextYear_Valid(object value, DateTime expected)
        => Assert.That(new NextYear().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PreviousDay_Valid(object value, DateTime expected)
        => Assert.That(new PreviousDay().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PreviousMonth_Valid(object value, DateTime expected)
        => Assert.That(new PreviousMonth().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PreviousYear_Valid(object value, DateTime expected)
        => Assert.That(new PreviousYear().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void SetTime_Valid(object value, string instant, DateTime expected)
        => Assert.That(new SetTime(() => instant).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void FloorHour_Valid(object value, DateTime expected)
        => Assert.That(new FloorHour().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CeilingHour_Valid(object value, DateTime expected)
        => Assert.That(new CeilingHour().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FloorMinute_Valid(object value, DateTime expected)
        => Assert.That(new FloorMinute().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CeilingMinute_Valid(object value, DateTime expected)
        => Assert.That(new CeilingMinute().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Forward_Valid(object value, string timeSpan, int times, DateTime expected)
        => Assert.That(new Forward(() => TimeOnly.Parse(timeSpan, CultureInfo.InvariantCulture), () => times)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Backward_Valid(object value, string timeSpan, int times, DateTime expected)
    => Assert.That(new Backward(() => TimeOnly.Parse(timeSpan, CultureInfo.InvariantCulture), () => times)
        .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Age_Valid_FixedCurrentDate(object? value, DateTime currentDate, int? expected)
    {
        var context = new EvaluationContext(new Dictionary<string, object?>
        {
            ["current-date"] = currentDate,
        });
        var expression = new Expression(new Age()).WithContext(context);

        Assert.That(expression.Evaluate(value), Is.EqualTo(expected));
    }

    [Test]
    public void Age_Default_Current_Date()
        => Assert.That(new Age().Evaluate(DateTime.Today.AddYears(-20)), Is.EqualTo(20));

    [Conformance]
    public void CatholicCalendar_Valid(object value, string @event, DateTime expected)
        => Assert.That(new CatholicCalendar(() => @event).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void CatholicCalendar_Null_UnknownEvent()
        => Assert.That(new CatholicCalendar(() => "unknown").Evaluate(2023), Is.Null);

    [Test]
    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Utc)]
    [TestCase(DateTimeKind.Unspecified)]
    public void CatholicCalendar_Valid_Kind(DateTimeKind expected)
        => Assert.That(((DateTime)new CatholicCalendar(() => "Christmas", () => expected.ToString()).Evaluate(2023)!).Kind, Is.EqualTo(expected));

    [Conformance]
    public void UtcToLocal_Valid_RomanceStandardTime(object value, string timeZone, DateTime expected)
        => Assert.That(new UtcToLocal(() => timeZone).Evaluate(value)
        , Is.EqualTo(expected));

    [Conformance]
    public void UtcToLocal_Valid_CityName(object value, string city, DateTime expected)
        => Assert.That(new UtcToLocal(() => city).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void LocalToUtc_Valid_RomanceStandardTime(object value, string timeZone, DateTime expected)
        => Assert.That(new LocalToUtc(() => timeZone).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void LocalToUtc_Valid_CityName(object value, string city, DateTime expected)
        => Assert.That(new LocalToUtc(() => city).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void DateTimeToDate_Valid(object value, DateTime expected)
        => Assert.That(new DateTimeToDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void DateTimeToDate_Valid_DateOnly(DateOnly value, DateTime expected)
        => Assert.That(new DateTimeToDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void DateTimeToDate_Valid_DateTimeOffset(DateTimeOffset value, DateTime expected)
        => Assert.That(new DateTimeToDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void DateTimeToDate_Invalid(object? value, DateTime? expected)
        => Assert.That(new DateTimeToDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NullToDate_Valid(object value, DateTime defaultValue, DateTime expected)
        => Assert.That(new NullToDate(() => defaultValue).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void InvalidToDate_Valid(object? value, DateTime defaultValue, DateTime? expected)
        => Assert.That(new InvalidToDate(() => defaultValue).Evaluate(value)
            , Is.EqualTo(expected == null ? new Null() : expected));

    [Test]
    [TestCase(typeof(DBNull), "2001-01-01")]
    public void InvalidToDate_DBNull_Valid(Type type, DateTime? expected)
        => Assert.That(new InvalidToDate(() => new DateTime(2001, 1, 1)).Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.EqualTo(new Null()));

    [Test]
    [TestCase("2018-02-01 00:00:00", "2018-02-01")]
    [TestCase("2018-02-01 07:00:00", "2018-02-01")]
    [TestCase("2018-02-12 07:00:00", "2018-02-01")]
    [TestCase(null, null)]
    [TestCase("(null)", null)]
    public void FirstOfMonth_Valid(object? value, DateTime? expected)
    {
        var function = new FirstOfMonth();
        var result = function.Evaluate(value);
        if (expected == new DateTime(1, 1, 1))
            Assert.That(result, Is.Null);
        else
            Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("2018-02-01 00:00:00", "2018-01-01")]
    [TestCase("2018-02-01 07:00:00", "2018-01-01")]
    [TestCase("2018-02-12 07:00:00", "2018-01-01")]
    [TestCase(null, null)]
    [TestCase("(null)", null)]
    public void FirstOfYear_Valid(object? value, DateTime? expected)
    {
        var function = new FirstOfYear();
        var result = function.Evaluate(value);
        if (expected == new DateTime(1, 1, 1))
            Assert.That(result, Is.Null);
        else
            Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("2018-02-01 00:00:00", "2018-02-28")]
    [TestCase("2018-02-01 07:00:00", "2018-02-28")]
    [TestCase("2018-02-12 07:00:00", "2018-02-28")]
    [TestCase("2020-02-12 07:00:00", "2020-02-29")]
    [TestCase(null, null)]
    [TestCase("(null)", null)]
    public void LastOfMonth_Valid(object? value, DateTime? expected)
    {
        var function = new LastOfMonth();
        var result = function.Evaluate(value);
        if (expected == new DateTime(1, 1, 1))
            Assert.That(result, Is.Null);
        else
            Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("2018-02-01 00:00:00", "2018-12-31")]
    [TestCase("2018-02-01 07:00:00", "2018-12-31")]
    [TestCase("2018-02-12 07:00:00", "2018-12-31")]
    [TestCase(null, null)]
    [TestCase("(null)", null)]
    public void LastOfYear_Valid(object? value, DateTime? expected)
    {
        var function = new LastOfYear();
        var result = function.Evaluate(value);
        if (expected == new DateTime(1, 1, 1))
            Assert.That(result, Is.Null);
        else
            Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("2019-11-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Brussels", "2019-11-01 20:58:00")]
    [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Brussels", "2019-10-01 21:58:00")]
    [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Moscow", "2019-10-01 22:58:00")]
    [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Pacific Standard Time", "2019-10-01 12:58:00")]
    public void TextToDateThenTimeAndUtcToLocal_Valid(string value, string format, string timeZone, DateTime expected)
    {
        var textToDateTime = new TextToDateTime(() => format);
        var utcToLocal = new UtcToLocal(() => timeZone);
        var result = utcToLocal.Evaluate(textToDateTime.Evaluate(value));
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(((DateTime)result!).Kind, Is.EqualTo(DateTimeKind.Unspecified));
        });
    }
}
