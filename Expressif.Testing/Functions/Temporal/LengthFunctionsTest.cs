using Expressif.Functions.Temporal;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class LengthFunctionsTest
{
    [Conformance]
    public void LengthOfYear_Integer_Valid(int year, int expected)
        => Assert.That(new LengthOfYear().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void LengthOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new LengthOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void LengthOfMonth_YearMonth_Valid(string yearMonth, int expected)
        => Assert.That(new LengthOfMonth().Evaluate((YearMonth)yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void LengthOfMonth_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new LengthOfMonth().Evaluate(dt), Is.EqualTo(expected));
}
