using Expressif.Predicates.Boolean;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Boolean;

[TestFixture]
public class TrueFalseTest
{
    [Conformance]
    public void True_Valid(object? value, bool expected)
        => Assert.That(new True().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TrueOrNull_Valid(object? value, bool expected)
        => Assert.That(new TrueOrNull().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void False_Valid(object? value, bool expected)
        => Assert.That(new False().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FalseOrNull_Valid(object? value, bool expected)
        => Assert.That(new FalseOrNull().Evaluate(value), Is.EqualTo(expected));
}
