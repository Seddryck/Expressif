namespace Expressif.Syntax;

/// <summary>
/// Provides the standard Expressif syntax parser.
/// </summary>
public sealed class ExpressionParser : IExpressionParser
{
    /// <summary>
    /// Parses source text into its canonical syntax representation.
    /// </summary>
    public static RootExpressionSyntax Parse(string text)
        => ExpressifSyntax.Parse(text);

    RootExpressionSyntax IExpressionParser.Parse(string text)
        => Parse(text);
}
