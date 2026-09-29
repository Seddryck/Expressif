namespace Expressif.Syntax;

/// <summary>
/// Parses Expressif source text into its syntax representation.
/// </summary>
public interface IExpressionParser
{
    RootExpressionSyntax Parse(string text);
}
