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
using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Library.Temporal;
using Expressif.Library.Text;

namespace Expressif.Testing.Text.Tokenization;

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
        => Assert.That(TestExpression.Create("tokenize-lines | count").Evaluate("a\r\nb\rc\n"), Is.EqualTo(4));
}
