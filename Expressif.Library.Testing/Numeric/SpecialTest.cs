using Expressif.Library.Numeric.Arithmetic;
using Expressif.Library.Numeric.Classification;
using Expressif.Library.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Numeric;

public class SpecialTest
{
    [Conformance]
    public void IsCodePoint_Valid(object? value, bool expected)
        => Assert.That(new CodePoint().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsWholeNumber_Valid(object? value, bool expected)
        => Assert.That(new WholeNumber().Evaluate(value), Is.EqualTo(expected));
}
