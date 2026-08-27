using Expressif.Predicates.Boolean;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Boolean;

[TestFixture]
public class TrueFalseTest
{
    [Conformance]
    public void IsTrue_Valid(object? value, bool expected)
        => Assert.That(new True().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsTrueOrNull_Valid(object? value, bool expected)
        => Assert.That(new TrueOrNull().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsFalse_Valid(object? value, bool expected)
        => Assert.That(new False().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsFalseOrNull_Valid(object? value, bool expected)
        => Assert.That(new FalseOrNull().Evaluate(value), Is.EqualTo(expected));
}
