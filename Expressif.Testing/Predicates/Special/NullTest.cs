using Expressif.Predicates.Special;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Special;

[TestFixture]
public class NullTest
{
    [Conformance]
    public void IsNull_Valid(object? value, bool expected)
    => Assert.That(new Null().Evaluate(value), Is.EqualTo(expected));
}

