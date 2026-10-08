using Expressif.Library.Boolean;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Boolean;

[TestFixture]
public class IdenticalToTest
{
    [Conformance]
    public void IsIdenticalTo_Valid(object? value, bool reference, bool expected)
        => Assert.That(new IdenticalTo(() => reference).Evaluate(value), Is.EqualTo(expected));
}

