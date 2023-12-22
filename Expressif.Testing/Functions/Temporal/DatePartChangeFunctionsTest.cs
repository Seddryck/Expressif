using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;
public class DatePartChangeFunctionsTest
{
    [Test]
    [TestCase(1900, 1903)]
    [TestCase(2000, 2000)]
    [TestCase(-45, 16)]
    [TestCase(800, 803)]
    [TestCase(12300, 2000)]
    public void ChangeOfYear_Integer_Valid(int year, int expected)
        => Assert.That(new ChangeOfYear(() => expected).Evaluate(year), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", 1903, "1903-01-01")]
    [TestCase("2000-01-01", 2000, "2000-01-01")]
    [TestCase("800-01-01", 2000, "2000-01-01")]
    [TestCase("2004-02-29", 2008, "2008-02-29")]
    [TestCase("2004-02-29", 2005, "2005-02-28")]
    public void ChangeOfYear_DateTime_Valid(DateTime dt, int newYear, DateTime expected)
        => Assert.That(new ChangeOfYear(() => newYear).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01", 1903, "1903-01")]
    [TestCase("2000-01", 2000, "2000-01")]
    [TestCase("0800-01", 2000, "2000-01")]
    public void ChangeOfYear_YearMonth_Valid(YearMonth yearMonth, int newYear, YearMonth expected)
        => Assert.That(new ChangeOfYear(() => newYear).Evaluate(yearMonth), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", 3, "1900-03-01")]
    [TestCase("2000-01-31", 3, "2000-03-31")]
    [TestCase("2000-01-31", 4, "2000-04-30")]
    [TestCase("2000-01-31", 2, "2000-02-29")]
    [TestCase("2001-01-31", 2, "2001-02-28")]
    public void ChangeOfMonth_DateTime_Valid(DateTime dt, int newMonth, DateTime expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", -1)]
    [TestCase("1900-01-01", 0)]
    [TestCase("1900-01-01", 13)]
    public void ChangeOfMonth_DateTime_Invalid(DateTime dt, int newMonth)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(dt)!.Equals(null), Is.True);

    [Test]
    [TestCase("1900-01", 3, "1900-03")]
    [TestCase("1900-01", 3, "1900-03")]
    [TestCase("2000-01", 1, "2000-01")]
    [TestCase("0800-01", 3, "0800-03")]
    public void ChangeOfMonth_YearMonth_Valid(YearMonth yearMonth, int newMonth, YearMonth expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(yearMonth), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01", -1)]
    [TestCase("1900-01", 0)]
    [TestCase("1900-01", 13)]
    public void ChangeOfMonth_YearMonth_Invalid(YearMonth yearMonth, int newMonth)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(yearMonth)!.Equals(null), Is.True);
}
