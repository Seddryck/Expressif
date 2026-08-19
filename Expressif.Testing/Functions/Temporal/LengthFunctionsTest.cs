using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expressif.Functions.Temporal;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class LengthFunctionsTest
{
    [Conformance]
    public void LengthOfYear_Valid_Integer(int year, int expected)
        => Assert.That(new LengthOfYear().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void LengthOfYear_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new LengthOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void LengthOfMonth_Valid_YearMonth(string yearMonth, int expected)
        => Assert.That(new LengthOfMonth().Evaluate((YearMonth)yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void LengthOfMonth_Valid_DateTime(DateTime dt, int expected)
        => Assert.That(new LengthOfMonth().Evaluate(dt), Is.EqualTo(expected));
}
