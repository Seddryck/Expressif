using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Values;

namespace Expressif.Testing.Functions.Temporal;
public class DatePartFunctionsTest
{
    [Test]
    [TestCase(1900, "1900")]
    [TestCase(2000, "2000")]
    [TestCase(-45, "-0045")]
    [TestCase(800, "0800")]
    [TestCase(12300, "12300")]
    public void Year_Integer_Valid(int year, string expected)
        => Assert.That(new Year().Evaluate(year), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01-01", "1900")]
    [TestCase("2000-01-01", "2000")]
    [TestCase("800-01-01", "0800")]
    public void Year_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new Year().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01", "1900")]
    [TestCase("2000-01", "2000")]
    [TestCase("0800-01", "0800")]
    public void Year_YearMonth_Valid(YearMonth yearMonth, string expected)
    => Assert.That(new Year().Evaluate(yearMonth), Is.EqualTo(expected));

    [Test]
    [TestCase(1, "01")]
    [TestCase(10, "10")]
    public void Month_Integer_Valid(int month, string expected)
    => Assert.That(new Month().Evaluate(month), Is.EqualTo(expected));

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(13)]
    public void Month_InvalidInteger_Valid(int month)
    => Assert.That(() => new Month().Evaluate(month), Throws.InstanceOf<ArgumentOutOfRangeException>());

    [Test]
    [TestCase("1900-01-01", "01")]
    [TestCase("2000-10-01", "10")]
    [TestCase("800-03-01", "03")]
    public void Month_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new Month().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1900-01", "01")]
    [TestCase("2000-10", "10")]
    [TestCase("0800-03", "03")]
    public void Month_YearMonth_Valid(YearMonth yearMonth, string expected)
    => Assert.That(new Month().Evaluate(yearMonth), Is.EqualTo(expected));


    [Test]
    [TestCase("1900-01-01", "01-01")]
    [TestCase("2000-10-01", "10-01")]
    [TestCase("800-03-17", "03-17")]
    public void MonthDay_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new MonthDay().Evaluate(dt), Is.EqualTo(expected));


    [Test]
    [TestCase("2000-01-01", "1999-W52")]
    [TestCase("2000-01-08", "2000-W01")]
    public void YearWeek_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new YearWeek().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1999-12-31", "1999-W52-5")]
    [TestCase("2000-01-01", "1999-W52-6")]
    [TestCase("2000-01-02", "1999-W52-7")]
    [TestCase("2000-01-03", "2000-W01-1")]
    public void YearWeekDay_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new YearWeekDay().Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("1999-12-31", "1999-362")]
    [TestCase("2000-01-01", "1999-363")]
    [TestCase("2000-01-02", "1999-364")]
    [TestCase("2000-01-03", "2000-001")]
    public void YearDay_DateTime_Valid(DateTime dt, string expected)
        => Assert.That(new YearDay().Evaluate(dt), Is.EqualTo(expected));
}
