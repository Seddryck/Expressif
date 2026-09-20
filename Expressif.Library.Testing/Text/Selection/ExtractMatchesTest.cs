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

namespace Expressif.Testing.Text.Selection;

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
        => Assert.That(TestExpression.Create(expression).Evaluate("a12b34"), Is.EqualTo(2));

    [Test]
    public void Evaluate_InvalidPattern_Throws()
        => Assert.Throws<RegexParseException>(() => new ExtractMatches(() => "[").Evaluate("abc"));

    [Test]
    public void Evaluate_PathologicalPattern_ThrowsTimeout()
        => Assert.Throws<RegexMatchTimeoutException>(() => new ExtractMatches(() => "(a+)+$")
            .Evaluate(new string('a', 10000) + "!"));

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
