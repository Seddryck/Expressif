using Expressif.Predicates.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Numeric;

[TestFixture]
public class ComparisonShortcutTest
{
    [Conformance]
    public void Zero_Valid(object? value, bool expected)
        => Assert.That(new Zero().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ZeroOrNull_Valid(object? value, bool expected)
        => Assert.That(new ZeroOrNull().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void One_Valid(object? value, bool expected)
        => Assert.That(new One().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Positive_Valid(object? value, bool expected)
        => Assert.That(new Positive().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PositiveOrZero_Valid(object? value, bool expected)
        => Assert.That(new PositiveOrZero().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Negative_Valid(object? value, bool expected)
        => Assert.That(new Negative().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NegativeOrZero_Valid(object? value, bool expected)
        => Assert.That(new NegativeOrZero().Evaluate(value), Is.EqualTo(expected));
}
