using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;
public class TimePartTextualFunctionsTest
{
    [Test]
    [TestCase("17:12:16", "17")]
    [TestCase("5:4:26", "05")]
    [TestCase("12:13:20", "12")]
    public void Hour_DateTime_Valid(string time, string expected)
    {
        var dt = new DateTime(2023, 12, 13);
        var timeOnly = TimeOnly.Parse(time);
        dt = dt.AddHours(timeOnly.Hour).AddMinutes(timeOnly.Minute).AddSeconds(timeOnly.Second);
        Assert.That(new Hour().Evaluate(dt), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("17:12:16", "17:12")]
    [TestCase("5:4:26", "05:04")]
    [TestCase("12:13:00", "12:13")]
    public void HourMinute_DateTime_Valid(string time, string expected)
    {
        var dt = new DateTime(2023, 12, 13);
        var timeOnly = TimeOnly.Parse(time);
        dt = dt.AddHours(timeOnly.Hour).AddMinutes(timeOnly.Minute).AddSeconds(timeOnly.Second);
        Assert.That(new HourMinute().Evaluate(dt), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("17:12:16", "17:12:16")]
    [TestCase("5:4:26", "05:04:26")]
    [TestCase("12:13:00", "12:13:00")]
    public void HourMinuteSecond_DateTime_Valid(string time, string expected)
    {
        var dt = new DateTime(2023, 12, 13);
        var timeOnly = TimeOnly.Parse(time);
        dt = dt.AddHours(timeOnly.Hour).AddMinutes(timeOnly.Minute).AddSeconds(timeOnly.Second);
        Assert.That(new HourMinuteSecond().Evaluate(dt), Is.EqualTo(expected));
    }
}
