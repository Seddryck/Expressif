using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expressif.Testing.Conformance;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class DatePartTextualFunctionsTest
{
    [Conformance]
    public void Year_Integer_Valid(int year, string expected)
        => Assert.That(new Year().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void Year_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new Year().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Year_YearMonth_Valid(YearMonth yearMonth, string expected)
    => Assert.That(new Year().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]

    public void Month_Integer_Invalid(int month, object? expected)
    => Assert.That(new Month().Evaluate(month), Is.EqualTo(expected));

    [Conformance]
    public void Month_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new Month().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Month_YearMonth_Valid(YearMonth yearMonth, string expected)
    => Assert.That(new Month().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void MonthDay_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new MonthDay().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoYearWeek_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new IsoYearWeek().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoYearWeekDay_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new IsoYearWeekDay().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoYearDay_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new IsoYearDay().Evaluate(dt), Is.EqualTo(expected));
}
