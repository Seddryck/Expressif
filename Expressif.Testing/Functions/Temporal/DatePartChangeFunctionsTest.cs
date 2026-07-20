using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;
using Expressif.Testing.Conformance;
using Expressif.Values.Special;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class DatePartChangeFunctionsTest
{
    [Conformance]
    public void ChangeOfYear_Integer_Valid(int value, int year, int expected)
        => Assert.That(new ChangeOfYear(() => year).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfYear_DateTime_Valid(DateTime dt, int newYear, DateTime expected)
        => Assert.That(new ChangeOfYear(() => newYear).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfYear_YearMonth_Valid(YearMonth yearMonth, int newYear, YearMonth expected)
        => Assert.That(new ChangeOfYear(() => newYear).Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_DateTime_Valid(DateTime dt, int newMonth, DateTime expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_DateTime_Invalid(DateTime dt, int newMonth, object? expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_YearMonth_Valid(YearMonth yearMonth, int newMonth, YearMonth expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void ChangeOfMonth_YearMonth_Invalid(YearMonth yearMonth, int newMonth, object? expected)
        => Assert.That(new ChangeOfMonth(() => newMonth).Evaluate(yearMonth), Is.EqualTo(expected));
}
