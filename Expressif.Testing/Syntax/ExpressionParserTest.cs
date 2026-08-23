using Expressif.Syntax;

namespace Expressif.Testing.Syntax;

[TestFixture]
public class ExpressionParserTest
{
    [Test]
    public void Parse_ValidSource_ReturnsSyntax()
        => Assert.That(ExpressionParser.Parse("upper"), Is.TypeOf<OpenExpressionSyntax>());
}
