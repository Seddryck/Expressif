using Expressif.Predicates.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Numeric;

public class SpecialTest
{
    [Conformance]
    public void Integer_Valid(object? value, bool expected)
        => Assert.That(new Integer().Evaluate(value), Is.EqualTo(expected));
}
