using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class CasingTest
{
    [Conformance]
    public void IsLowerCase_Valid_Text(object? value, bool expected)
        => Assert.That(new LowerCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsUpperCase_Valid_Text(object? value, bool expected)
        => Assert.That(new UpperCase().Evaluate(value), Is.EqualTo(expected));
}
