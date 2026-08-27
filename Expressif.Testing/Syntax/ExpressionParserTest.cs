using Expressif.Syntax;

namespace Expressif.Testing.Syntax;

[TestFixture]
public class ExpressionParserTest
{
    [Test]
    public void Parse_ValidSource_ReturnsSyntax()
        => Assert.That(ExpressionParser.Parse("upper"), Is.TypeOf<OpenExpressionSyntax>());

    [TestCase("even |and greater-than(5)")]
    [TestCase("even |or less-than(0)")]
    [TestCase("even |xor odd")]
    public void Parse_LowercaseBinaryOperator_ReturnsSyntax(string source)
        => Assert.That(ExpressionParser.Parse(source), Is.TypeOf<OpenExpressionSyntax>());

    [Test]
    public void Parse_OperatorTextInsideQuotedLiteral_DoesNotNormalizeLiteral()
    {
        var syntax = ExpressionParser.Parse("equal-to(\"|and\")");
        var quoted = syntax.Children.SelectMany(DescendantsAndSelf).OfType<QuotedLiteralSyntax>().Single();

        Assert.That(quoted.Value, Is.EqualTo("|and"));
    }

    private static IEnumerable<SyntaxNode> DescendantsAndSelf(SyntaxNode node)
        => new[] { node }.Concat(node.Children.SelectMany(DescendantsAndSelf));
}
