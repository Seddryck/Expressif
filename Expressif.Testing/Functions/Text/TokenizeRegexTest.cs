using System.Text.RegularExpressions;
using Expressif.Functions;
using Expressif.Functions.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class TokenizeRegexTest
{
    [Conformance]
    public void TokenizeRegex_Valid(string? value, string pattern, string[] expected)
    {
        IFunction<string?, string[]?> function = new TokenizeRegex(() => pattern);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [TestCase("tokenize-regex(\",\") | count")]
    [TestCase("tokenize-regex(pattern := \",\") | count")]
    public void Evaluate_BindsPatternAndComposes(string expression)
        => Assert.That(Expression.Create(expression).Evaluate("a,b,c"), Is.EqualTo(3));

    [Test]
    public void Evaluate_InvalidPattern_Throws()
        => Assert.Throws<RegexParseException>(() => new TokenizeRegex(() => "[").Evaluate("abc"));

    [Test]
    public void Evaluate_ReusesFunctionWithUpdatedPattern()
    {
        var pattern = ",";
        var function = new TokenizeRegex(() => pattern);
        Assert.That(function.Evaluate("a,b;c"), Is.EqualTo(new[] { "a", "b;c" }));
        pattern = ";";
        Assert.That(function.Evaluate("a,b;c"), Is.EqualTo(new[] { "a,b", "c" }));
    }
}
