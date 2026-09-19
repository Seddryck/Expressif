namespace Expressif.Syntax;

/// <summary>
/// Parses Expressif source text into its syntax representation.
/// </summary>
public interface IExpressionParser
{
    /// <summary>
    /// Parses Expressif source text into a syntax tree without binding it to runtime behavior.
    /// </summary>
    /// <param name="text">The Expressif source text to parse.</param>
    /// <returns>The root of the parsed syntax tree.</returns>
    RootExpressionSyntax Parse(string text);
}
