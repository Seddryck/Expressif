using Expressif.Library.Special;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Special;

[TestFixture]
public class NullTest
{
    [Conformance]
    public void IsNotNull_Valid(object? value, bool expected)
        => Assert.That(new NotNull().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsNull_Valid(object? value, bool expected)
    => Assert.That(new Null().Evaluate(value), Is.EqualTo(expected));
}

