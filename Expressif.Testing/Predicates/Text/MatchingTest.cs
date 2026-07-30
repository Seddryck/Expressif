using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class MatchingTest
{
    [Conformance]
    public void MatchesNumeric_Valid(object? value, bool expected)
        => Assert.That(new MatchesNumeric().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesNumeric_Valid_CultureSpecific(object? value, string culture, bool expected)
        => Assert.That(new MatchesNumeric(() => culture).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDate_Valid(object? value, bool expected)
        => Assert.That(new MatchesDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDate_Valid_Date(string value, bool expected)
        => Assert.That(new MatchesDate().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDate_Valid_DateTime(DateTime value, bool expected)
        => Assert.That(new MatchesDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDate_Valid_Time(string value, bool expected)
        => Assert.That(new MatchesDate().Evaluate(TimeOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDate_Valid_CultureSpecific(object? value, string culture, bool expected)
        => Assert.That(new MatchesDate(() => culture).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDateTime_Valid(object? value, bool expected)
        => Assert.That(new MatchesDateTime().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDateTime_Valid_CultureSpecific(object? value, string culture, bool expected)
        => Assert.That(new MatchesDateTime(() => culture).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesDateTime_Valid_DateTime(DateTime value, bool expected)
        => Assert.That(new MatchesDateTime().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesTime_Valid(object? value, bool expected)
        => Assert.That(new MatchesTime().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesTime_Valid_CultureSpecific(object? value, string culture, bool expected)
        => Assert.That(new MatchesTime(() => culture).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesTime_Valid_DateTime(DateTime value, bool expected)
        => Assert.That(new MatchesTime().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MatchesTime_Valid_Date(string value, bool expected)
        => Assert.That(new MatchesTime().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void MatchesTime_Valid_Time(string value, bool expected)
        => Assert.That(new MatchesTime().Evaluate(TimeOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void MatchesTime_Valid_TimeSpan(string value, bool expected)
        => Assert.That(new MatchesTime().Evaluate(TimeSpan.Parse(value)), Is.EqualTo(expected));
}
