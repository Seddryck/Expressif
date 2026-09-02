using Expressif.Predicates.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Numeric;

public class SpecialTest
{
    [Conformance]
    public void IsCodePoint_Valid(object? value, bool expected)
        => Assert.That(new CodePoint().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsWholeNumber_Valid(object? value, bool expected)
        => Assert.That(new WholeNumber().Evaluate(value), Is.EqualTo(expected));
}
