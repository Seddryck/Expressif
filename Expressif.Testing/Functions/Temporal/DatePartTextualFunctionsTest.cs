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
    public void Year_Valid_Integer(int year, string expected)
        => Assert.That(new Year().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void Year_Valid_DateTime(DateTime dt, string expected)
        => Assert.That(new Year().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Year_Valid_YearMonth(YearMonth yearMonth, string expected)
    => Assert.That(new Year().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]

    public void Month_Invalid_Integer(int month, object? expected)
    => Assert.That(new Month().Evaluate(month), Is.EqualTo(expected));

    [Conformance]
    public void Month_Valid_DateTime(DateTime dt, string expected)
        => Assert.That(new Month().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Month_Valid_YearMonth(YearMonth yearMonth, string expected)
    => Assert.That(new Month().Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void MonthDay_Valid_DateTime(DateTime dt, string expected)
        => Assert.That(new MonthDay().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoYearWeek_Valid_DateTime(DateTime dt, string expected)
        => Assert.That(new IsoYearWeek().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoYearWeekDay_Valid_DateTime(DateTime dt, string expected)
        => Assert.That(new IsoYearWeekDay().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsoYearDay_Valid_DateTime(DateTime dt, string expected)
        => Assert.That(new IsoYearDay().Evaluate(dt), Is.EqualTo(expected));
}
