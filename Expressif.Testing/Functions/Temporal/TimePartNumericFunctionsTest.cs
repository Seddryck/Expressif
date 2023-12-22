using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;
public class TimePartNumericFunctionsTest
{
    [Test]
    [TestCase("1900-01-01 17:12:26", 17)]
    [TestCase("2000-01-01 05:02:12", 5)]
    [TestCase("800-01-01 12:00:00", 12)]
    public void HourOfDay_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new HourOfDay().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01 17:12:26", 12)]
    [TestCase("2000-01-01 05:02:12", 2)]
    [TestCase("800-01-01 12:00:00", 0)]
    public void MinuteOfHour_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new MinuteOfHour().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01 17:12:26", 17 * 60 + 12)]
    [TestCase("2000-01-01 05:02:12", 5 * 60 + 2)]
    [TestCase("800-01-01 12:00:00", 12 * 60)]
    public void MinuteOfDay_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new MinuteOfDay().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01 17:12:26", 26)]
    [TestCase("2000-01-01 05:02:12", 12)]
    [TestCase("800-01-01 12:00:00", 0)]
    public void SecondOfMinute_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new SecondOfMinute().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01 17:12:26", 12*60 + 26)]
    [TestCase("2000-01-01 05:02:12", 2*60+12)]
    [TestCase("800-01-01 12:00:00", 0*60 + 0)]
    public void SecondOfHour_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new SecondOfHour().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01 17:12:26", 17*3600 + 12 * 60 + 26)]
    [TestCase("2000-01-01 05:02:12", 5 * 3600 + 2 * 60 + 12)]
    [TestCase("800-01-01 12:00:00", 12 * 3600 + 0 * 60 + 0)]
    public void SecondOfDay_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new SecondOfDay().Evaluate(dt), Is.EqualTo(expected));
}
