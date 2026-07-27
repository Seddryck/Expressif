using Expressif.Predicates.Boolean;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Boolean;

[TestFixture]
public class IdenticalToTest
{
    [Conformance]
    public void IdenticalTo_Valid(object? value, bool reference, bool expected)
        => Assert.That(new IdenticalTo(() => reference).Evaluate(value), Is.EqualTo(expected));
}

