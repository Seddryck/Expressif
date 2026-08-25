using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class OnTheInstantTest
{
    [Conformance]
    public void IsOnTheDay_Valid_DateTime(object? value, bool expected)
        => Assert.That(new OnTheDay().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheDay_Valid_Date(string value, bool expected)
        => Assert.That(new OnTheDay().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheDay_Valid_Special(string text, bool expected)
        => Assert.That(new OnTheDay().Evaluate(text), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheHour_Valid_DateTime(object? value, bool expected)
        => Assert.That(new OnTheHour().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheHour_Valid_Date(string value, bool expected)
        => Assert.That(new OnTheHour().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheHour_Valid_Special(string text, bool expected)
        => Assert.That(new OnTheHour().Evaluate(text), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheMinute_Valid_DateTime(object? value, bool expected)
        => Assert.That(new OnTheMinute().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheMinute_Valid_Date(string value, bool expected)
        => Assert.That(new OnTheMinute().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void IsOnTheMinute_Valid_Special(string text, bool expected)
        => Assert.That(new OnTheMinute().Evaluate(text), Is.EqualTo(expected));
}
