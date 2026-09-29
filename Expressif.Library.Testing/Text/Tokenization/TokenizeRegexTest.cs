using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Character;
using Expressif.Library.Text.Concatenation;
using Expressif.Library.Text.Conversion;
using Expressif.Library.Text.Counting;
using Expressif.Library.Text.Encoding;
using Expressif.Library.Text.Filtering;
using Expressif.Library.Text.Masking;
using Expressif.Library.Text.Normalization;
using Expressif.Library.Text.Padding;
using Expressif.Library.Text.Partitioning;
using Expressif.Library.Text.Selection;
using Expressif.Library.Text.Tokenization;
using System.Text.RegularExpressions;
using Expressif.Functions;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Text.Tokenization;

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
        => Assert.That(TestExpression.Create(expression).Evaluate("a,b,c"), Is.EqualTo(3));

    [Test]
    public void Evaluate_InvalidPattern_Throws()
        => Assert.Throws<RegexParseException>(() => new TokenizeRegex(() => "[").Evaluate("abc"));

    [Test]
    public void Evaluate_PathologicalPattern_ThrowsTimeout()
        => Assert.Throws<RegexMatchTimeoutException>(() => new TokenizeRegex(() => "(a+)+$")
            .Evaluate(new string('a', 10000) + "!"));

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
