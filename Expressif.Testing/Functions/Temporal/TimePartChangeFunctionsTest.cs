using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class TimePartChangeFunctionsTest
{
    [Conformance]
    public void ChangeOfHour_DateTime_Valid(DateTime dt, int newHour, DateTime expected)
        => Assert.That(new ChangeOfHour(() => newHour).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2001-01-01 17:12:16", -1)]
    [TestCase("2001-01-01 17:12:16", 24)]
    public void ChangeOfHour_DateTime_Invalid(DateTime dt, int newHour)
        => Assert.That(new ChangeOfHour(() => newHour).Evaluate(dt)!.Equals(null), Is.True);

    [Conformance]
    public void ChangeOfMinute_DateTime_Valid(DateTime dt, int newMinute, DateTime expected)
        => Assert.That(new ChangeOfMinute(() => newMinute).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2001-01-01 17:12:16", -1)]
    [TestCase("2001-01-01 17:12:16", 60)]
    public void ChangeOfMinute_DateTime_Invalid(DateTime dt, int newMinute)
        => Assert.That(new ChangeOfMinute(() => newMinute).Evaluate(dt)!.Equals(null), Is.True);

    [Conformance]
    public void ChangeOfSecond_DateTime_Valid(DateTime dt, int newSecond, DateTime expected)
        => Assert.That(new ChangeOfSecond(() => newSecond).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2001-01-01 17:12:16", -1)]
    [TestCase("2001-01-01 17:12:16", 60)]
    public void ChangeOfSecond_DateTime_Invalid(DateTime dt, int newSecond)
        => Assert.That(new ChangeOfSecond(() => newSecond).Evaluate(dt)!.Equals(null), Is.True);
}
