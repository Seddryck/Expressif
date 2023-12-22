using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;
public class DatePartNumericFunctionsTest
{
    [Test]
    [TestCase(1900, 1900)]
    [TestCase(2000, 2000)]
    [TestCase(-45, -0045)]
    [TestCase(800, 800)]
    [TestCase(12300, 12300)]
    public void YearOfEra_Integer_Valid(int year, int expected)
        => Assert.That(new YearOfEra().Evaluate(year), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", 1900)]
    [TestCase("2000-01-01", 2000)]
    [TestCase("800-01-01", 800)]
    public void YearOfEra_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new YearOfEra().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01", 1900)]
    [TestCase("2000-01", 2000)]
    [TestCase("0800-01", 800)]
    public void YearOfEra_YearMonth_Valid(YearMonth yearMonth, int expected)
    => Assert.That(new YearOfEra().Evaluate(yearMonth), Is.EqualTo(expected));

    [Test]
    [TestCase(1)]
    [TestCase(10)]
    public void MonthOfYear_Integer_Valid(int month)
    => Assert.That(new MonthOfYear().Evaluate(month), Is.Null);

    [Test]
    [TestCase("1900-01-01", 1)]
    [TestCase("2000-10-01", 10)]
    [TestCase("800-03-01", 3)]
    public void MonthOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new MonthOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01", 1)]
    [TestCase("2000-10", 10)]
    [TestCase("0800-03", 3)]
    public void MonthOfYear_YearMonth_Valid(YearMonth yearMonth, int expected)
    => Assert.That(new MonthOfYear().Evaluate(yearMonth), Is.EqualTo(expected));

    [Test]
    [TestCase("2000-01-01", 6)]
    [TestCase("2000-01-02", 7)]
    [TestCase("2000-01-03", 1)]
    public void DayOfWeek_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new Expressif.Functions.Temporal.DayOfWeek().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", 1)]
    [TestCase("2000-10-28", 28)]
    [TestCase("800-03-17", 17)]
    public void DayOfMonth_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new DayOfMonth().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", 1)]
    [TestCase("2000-02-01", 32)]
    [TestCase("800-03-17", 77)]
    public void DayOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new DayOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1999-12-31", 362)]
    [TestCase("2000-01-01", 363)]
    [TestCase("2000-01-02", 364)]
    [TestCase("2000-01-03", 001)]
    public void IsoDayOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new IsoDayOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1999-12-31", 52)]
    [TestCase("2000-01-01", 52)]
    [TestCase("2000-01-02", 52)]
    [TestCase("2000-01-03", 01)]
    public void IsoWeekOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new IsoWeekOfYear().Evaluate(dt), Is.EqualTo(expected));
}
