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
        => ExpressifSyntax.Parse(NormalizeGroupingMapOperators(
            NormalizeBinaryOperators(NormalizeEnclosingRootReferences(text))));

    private static string NormalizeEnclosingRootReferences(string text)
    {
        const string marker = "^^.";
        var result = new System.Text.StringBuilder(text.Length);
        var quoted = false;
        for (var index = 0; index < text.Length; index++)
        {
            if (IsUnescapedQuote(text, index))
                quoted = !quoted;

            if (quoted || !text.AsSpan(index).StartsWith(marker, StringComparison.Ordinal))
            {
                result.Append(text[index]);
                continue;
            }

            var fieldStart = index + marker.Length;
            var fieldEnd = fieldStart;
            while (fieldEnd < text.Length && IsBareFieldCharacter(text[fieldEnd]))
                fieldEnd++;

            if (fieldEnd == fieldStart)
            {
                result.Append(text[index]);
                continue;
            }

            var field = text[fieldStart..fieldEnd];
            result.Append("enclosing-root-field(\"").Append(field).Append("\")");
            index = fieldEnd - 1;
        }
        return result.ToString();
    }

    private static bool IsBareFieldCharacter(char value)
        => char.IsLetterOrDigit(value) || value is '_' or '-';

    private static string NormalizeGroupingMapOperators(string text)
    {
        var result = new System.Text.StringBuilder(text.Length);
        var quoted = false;
        for (var index = 0; index < text.Length; index++)
        {
            if (IsUnescapedQuote(text, index))
                quoted = !quoted;

            if (quoted || !IsGroupingMapOperator(text, index))
            {
                result.Append(text[index]);
                continue;
            }

            index += 3;
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            var start = index;
            index = FindGroupingMapExpressionEnd(text, start);
            var expression = text[start..index].TrimEnd();
            result.Append("| group-map-shorthand(").Append(expression).Append(')');
            index--;
        }
        return result.ToString();
    }

    private static bool IsUnescapedQuote(string text, int index)
        => text[index] == '"' && (index == 0 || text[index - 1] != '\\');

    private static bool IsGroupingMapOperator(string text, int index)
        => index + 2 < text.Length && text.AsSpan(index, 3) is "|#>";

    private static int FindGroupingMapExpressionEnd(string text, int start)
    {
        var depth = 0;
        var quoted = false;
        for (var index = start; index < text.Length; index++)
        {
            var current = text[index];
            if (IsUnescapedQuote(text, index))
                quoted = !quoted;
            if (quoted)
                continue;
            depth += current is '(' or '{' ? 1 : current is ')' or '}' ? -1 : 0;
            if (current == '|' && depth == 0)
                return index;
        }
        return text.Length;
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
