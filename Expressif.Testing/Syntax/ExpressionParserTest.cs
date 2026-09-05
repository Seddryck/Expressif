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

    [Test]
    public void Parse_EnclosingRootReference_NormalizesOutsideQuotedLiterals()
    {
        var syntax = ExpressionParser.Parse("greater-than(^^.threshold) | suffix(\"^^.literal\")");
        var descendants = syntax.Children.SelectMany(DescendantsAndSelf).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(descendants.OfType<FunctionCallSyntax>()
                .Any(call => call.Name == "enclosing-root-field"), Is.True);
            Assert.That(descendants.OfType<QuotedLiteralSyntax>()
                .Any(literal => literal.Value == "^^.literal"), Is.True);
        });
    }

    private static IEnumerable<SyntaxNode> DescendantsAndSelf(SyntaxNode node)
        => new[] { node }.Concat(node.Children.SelectMany(DescendantsAndSelf));
}
