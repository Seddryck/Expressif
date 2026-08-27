using Expressif.Functions.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Array;

public class CardinalityTest
{
    [Conformance]
    public void Cardinality_Valid(object?[] value, int expected)
        => Assert.That(new Cardinality().Evaluate(value), Is.EqualTo(expected));
}
