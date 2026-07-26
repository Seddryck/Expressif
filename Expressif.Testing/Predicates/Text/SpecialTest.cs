using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class SpecialTest
{
    [Conformance]
    public void Empty_Valid(object? value, bool expected)
        => Assert.That(new Empty().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void EmptyOrNull_Valid(object? value, bool expected)
        => Assert.That(new EmptyOrNull().Evaluate(value), Is.EqualTo(expected));
}
