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
    public void YearOfEra_Integer_Valid(int year, int expected)
        => Assert.That(new YearOfEra().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void YearOfEra_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new YearOfEra().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void YearOfEra_YearMonth_Valid(YearMonth yearMonth, int expected)
    => Assert.That(new YearOfEra().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void MonthOfYear_Integer_Invalid(int month, object? expected)
    => Assert.That(new MonthOfYear().Evaluate(month), Is.EqualTo(expected));

    [Conformance]
    public void MonthOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new MonthOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void MonthOfYear_YearMonth_Valid(YearMonth yearMonth, int expected)
    => Assert.That(new MonthOfYear().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void DayOfWeek_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new Expressif.Functions.Temporal.DayOfWeek().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void DayOfMonth_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new DayOfMonth().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void DayOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new DayOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoDayOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new IsoDayOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoWeekOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new IsoWeekOfYear().Evaluate(dt), Is.EqualTo(expected));
}
