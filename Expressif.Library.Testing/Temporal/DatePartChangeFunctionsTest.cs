using Expressif.Library.Temporal.Calendar;
using Expressif.Library.Temporal.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Library.Temporal;
using Expressif.Testing.Conformance;
using Expressif.Values;
using Expressif.Values.Special;

namespace Expressif.Testing.Temporal;

[TestFixture]
public class DatePartChangeFunctionsTest
{
    [Conformance]
    public void ChangeOfYear_Valid_Integer(int value, int year, int expected)
        => Assert.That(new ChangeOfYear(() => year).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfYear_Valid_DateTime(DateTime dt, int newYear, DateTime expected)
        => Assert.That(new ChangeOfYear(() => newYear).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfYear_Valid_YearMonth(YearMonth yearMonth, int newYear, YearMonth expected)
        => Assert.That(new ChangeOfYear(() => newYear).Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_Valid_DateTime(DateTime dt, int newMonth, DateTime expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_Invalid_DateTime(DateTime dt, int newMonth, object? expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_Valid_YearMonth(YearMonth yearMonth, int newMonth, YearMonth expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_Invalid_YearMonth(YearMonth yearMonth, int newMonth, object? expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(yearMonth), Is.EqualTo(expected));
}
