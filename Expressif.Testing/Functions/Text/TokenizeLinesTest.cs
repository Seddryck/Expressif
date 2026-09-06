using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Functions.Text;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class TokenizeLinesTest
{
    [Conformance]
    public void TokenizeLines_Valid(string? value, string[] expected)
    {
        IFunction<string?, string[]?> function = new TokenizeLines();
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Test]
    public void Evaluate_ComposesWithArrayFunction()
        => Assert.That(Expression.Create("tokenize-lines | count").Evaluate("a\r\nb\rc\n"), Is.EqualTo(4));
}
