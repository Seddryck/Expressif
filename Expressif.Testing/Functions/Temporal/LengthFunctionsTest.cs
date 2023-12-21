using Expressif.Functions.Temporal;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Temporal;

public class LengthFunctionsTest
{
    [Test]
    [TestCase(1900, 365)]
    [TestCase(1999, 365)]
    [TestCase(2000, 366)]
    [TestCase(2023, 365)]
    [TestCase(2024, 366)]
    public void LengthOfYear_Integer_Valid(int year, int expected)
        => Assert.That(new LengthOfYear().Evaluate(year), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-05-25", 365)]
    [TestCase("1999-05-25", 365)]
    [TestCase("2000-05-25", 366)]
    [TestCase("2023-05-25", 365)]
    [TestCase("2024-05-25", 366)]
    public void LengthOfYear_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new LengthOfYear().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-02", 28)]
    [TestCase("1999-02", 28)]
    [TestCase("2000-02", 29)]
    [TestCase("2023-02", 28)]
    [TestCase("2024-02", 29)]
    [TestCase("2024-01", 31)]
    [TestCase("2024-03", 31)]
    [TestCase("2024-04", 30)]
    [TestCase("2024-12", 31)]
    public void LengthOfMonth_YearMonth_Valid(string yearMonth, int expected)
=> Assert.That(new LengthOfMonth().Evaluate((YearMonth)yearMonth), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-02-15", 28)]
    [TestCase("1999-02-15", 28)]
    [TestCase("2000-02-15", 29)]
    [TestCase("2023-02-15", 28)]
    [TestCase("2024-02-15", 29)]
    [TestCase("2024-01-15", 31)]
    [TestCase("2024-03-15", 31)]
    [TestCase("2024-04-15", 30)]
    [TestCase("2024-12-15", 31)]
    public void LengthOfMonth_DateTime_Valid(DateTime dt, int expected)
        => Assert.That(new LengthOfMonth().Evaluate(dt), Is.EqualTo(expected));
}
