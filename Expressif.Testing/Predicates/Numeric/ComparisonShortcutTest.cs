using Expressif.Predicates.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Numeric;

[TestFixture]
public class ComparisonShortcutTest
{
    [Conformance]
    public void IsZero_Valid(object? value, bool expected)
        => Assert.That(new Zero().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsZeroOrNull_Valid(object? value, bool expected)
        => Assert.That(new ZeroOrNull().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsOne_Valid(object? value, bool expected)
        => Assert.That(new One().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsPositive_Valid(object? value, bool expected)
        => Assert.That(new Positive().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsPositiveOrZero_Valid(object? value, bool expected)
        => Assert.That(new PositiveOrZero().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsNegative_Valid(object? value, bool expected)
        => Assert.That(new Negative().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsNegativeOrZero_Valid(object? value, bool expected)
        => Assert.That(new NegativeOrZero().Evaluate(value), Is.EqualTo(expected));
}
