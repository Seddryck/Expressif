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
        => ExpressifSyntax.Parse(NormalizeGroupingMapOperators(NormalizeBinaryOperators(text)));

    private static string NormalizeGroupingMapOperators(string text)
    {
        var result = new System.Text.StringBuilder(text.Length);
        var quoted = false;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '"' && (index == 0 || text[index - 1] != '\\'))
                quoted = !quoted;

            if (quoted || index + 2 >= text.Length || text.AsSpan(index, 3) is not "|#>")
            {
                result.Append(text[index]);
                continue;
            }

            index += 3;
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            var start = index;
            var depth = 0;
            var innerQuoted = false;
            for (; index < text.Length; index++)
            {
                var current = text[index];
                if (current == '"' && (index == start || text[index - 1] != '\\'))
                    innerQuoted = !innerQuoted;
                if (innerQuoted)
                    continue;
                if (current is '(' or '{')
                    depth++;
                else if (current is ')' or '}')
                    depth--;
                else if (current == '|' && depth == 0)
                    break;
            }

            var expression = text[start..index].TrimEnd();
            result.Append("| group-map-shorthand(").Append(expression).Append(')');
            index--;
        }
        return result.ToString();
    }

    private static string NormalizeBinaryOperators(string text)
    {
        var normalized = text.ToCharArray();
        var quoted = false;
        for (var index = 0; index < normalized.Length; index++)
        {
            if (normalized[index] == '"' && (index == 0 || normalized[index - 1] != '\\'))
            {
                quoted = !quoted;
                continue;
            }

            if (quoted || normalized[index] != '|')
                continue;

            foreach (var candidate in new[] { "and", "xor", "or" })
            {
                if (index + candidate.Length >= normalized.Length
                    || !text.AsSpan(index + 1, candidate.Length).Equals(candidate, StringComparison.OrdinalIgnoreCase)
                    || (index + candidate.Length + 1 < normalized.Length
                        && char.IsLetterOrDigit(normalized[index + candidate.Length + 1])))
                    continue;

                for (var offset = 0; offset < candidate.Length; offset++)
                    normalized[index + offset + 1] = char.ToUpperInvariant(normalized[index + offset + 1]);
                break;
            }
        }

        return new string(normalized);
    }

    RootExpressionSyntax IExpressionParser.Parse(string text)
        => Parse(text);
}
