using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expressif.Testing.Conformance;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class DatePartNumericFunctionsTest
{
    [Conformance]
    public void YearOfEra_Valid_Integer(int year, int expected)
        => Assert.That(new YearOfEra().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void YearOfEra_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new YearOfEra().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void YearOfEra_Valid_YearMonth(YearMonth yearMonth, int expected)
    => Assert.That(new YearOfEra().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void MonthOfYear_Invalid_Integer(int month, object? expected)
    => Assert.That(new MonthOfYear().Evaluate(month), Is.EqualTo(expected));

    [Conformance]
    public void MonthOfYear_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new MonthOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void MonthOfYear_Valid_YearMonth(YearMonth yearMonth, int expected)
    => Assert.That(new MonthOfYear().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void DayOfWeek_Valid_DateTime(DateTime dt, int expected)
    => Assert.That(new Expressif.Functions.Temporal.DayOfWeek().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void DayOfMonth_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new DayOfMonth().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void DayOfYear_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new DayOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoDayOfYear_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new IsoDayOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoWeekOfYear_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new IsoWeekOfYear().Evaluate(dt), Is.EqualTo(expected));
}
