using Expressif.Predicates.Temporal;
using Expressif.Values;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class LeapYearPredicatesTest
{
    [Conformance]
    public void LeapYear_Valid_Year(int year, bool expected)
        => Assert.That(new LeapYear().Evaluate(year), Is.EqualTo(expected));

    [Conformance]
    public void LeapYear_Valid_YearMonth(string yearMonth, bool expected)
        => Assert.That(new LeapYear().Evaluate((YearMonth)yearMonth), Is.EqualTo(expected));

    [Conformance]
    public void LeapYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new LeapYear().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void LeapYear_Valid_Text(string text, bool expected)
        => Assert.That(new LeapYear().Evaluate(text), Is.EqualTo(expected));
}
