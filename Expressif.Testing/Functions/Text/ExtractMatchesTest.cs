using System.Text.RegularExpressions;
using Expressif.Functions;
using Expressif.Functions.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class ExtractMatchesTest
{
    [Conformance]
    public void ExtractMatches_Valid(string? value, string pattern, string[] expected)
    {
        IFunction<string?, string[]?> function = new ExtractMatches(() => pattern);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [TestCase("extract-matches(\"[0-9]+\") | count")]
    [TestCase("extract-matches(pattern := \"[0-9]+\") | count")]
    public void Evaluate_BindsPatternAndComposes(string expression)
        => Assert.That(Expression.Create(expression).Evaluate("a12b34"), Is.EqualTo(2));

    [Test]
    public void Evaluate_InvalidPattern_Throws()
        => Assert.Throws<RegexParseException>(() => new ExtractMatches(() => "[").Evaluate("abc"));

    [Test]
    public void Evaluate_ReusesFunctionWithUpdatedPattern()
    {
        var pattern = "[0-9]+";
        var function = new ExtractMatches(() => pattern);
        Assert.That(function.Evaluate("a12b34"), Is.EqualTo(new[] { "12", "34" }));
        pattern = "[a-z]+";
        Assert.That(function.Evaluate("a12b34"), Is.EqualTo(new[] { "a", "b" }));
    }
}
