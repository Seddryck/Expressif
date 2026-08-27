using Expressif.Functions.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class SlugFunctionsTest
{
    [Conformance]
    public void Slug_Valid(object? value, string expected)
        => Assert.That(new Slug().Evaluate(value), Is.EqualTo(expected));
}
