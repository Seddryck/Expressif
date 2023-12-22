using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;
public class TimePartChangeFunctionsTest
{
    [Test]
    [TestCase("2001-01-01 17:12:16", 17, "2001-01-01 17:12:16")]
    [TestCase("1901-01-01 05:04:26", 12, "1901-01-01 12:04:26")]
    [TestCase("0800-01-01 12:00:00", 14, "0800-01-01 14:00:00")]
    public void ChangeOfHour_DateTime_Valid(DateTime dt, int newYear, DateTime expected)
        => Assert.That(new ChangeOfHour(() => newYear).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2001-01-01 17:12:16", -1)]
    [TestCase("2001-01-01 17:12:16", 24)]
    public void ChangeOfHour_YearMonth_Invalid(DateTime dt, int newMonth)
        => Assert.That(new ChangeOfHour(() => newMonth).Evaluate(dt)!.Equals(null), Is.True);

    [Test]
    [TestCase("2001-01-01 17:12:16", 44, "2001-01-01 17:44:16")]
    [TestCase("1901-01-01 05:04:26", 00, "1901-01-01 05:00:26")]
    [TestCase("0800-01-01 12:00:00", 30, "0800-01-01 12:30:00")]
    public void ChangeOfMinute_DateTime_Valid(DateTime dt, int newYear, DateTime expected)
        => Assert.That(new ChangeOfMinute(() => newYear).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2001-01-01 17:12:16", -1)]
    [TestCase("2001-01-01 17:12:16", 60)]
    public void ChangeOfMinute_YearMonth_Invalid(DateTime dt, int newMonth)
        => Assert.That(new ChangeOfMinute(() => newMonth).Evaluate(dt)!.Equals(null), Is.True);

    [Test]
    [TestCase("2001-01-01 17:12:16", 17, "2001-01-01 17:12:17")]
    [TestCase("1901-01-01 05:04:26", 0, "1901-01-01 05:04:00")]
    [TestCase("0800-01-01 12:00:00", 10, "0800-01-01 12:00:10")]
    public void ChangeOfSecond_DateTime_Valid(DateTime dt, int newSecond, DateTime expected)
        => Assert.That(new ChangeOfSecond(() => newSecond).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2001-01-01 17:12:16", -1)]
    [TestCase("2001-01-01 17:12:16", 60)]
    public void ChangeOfSecond_YearMonth_Invalid(DateTime dt, int newMonth)
        => Assert.That(new ChangeOfSecond(() => newMonth).Evaluate(dt)!.Equals(null), Is.True);
}
