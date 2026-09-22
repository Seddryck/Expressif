using System.Text;
using Expressif.Values;

namespace Expressif.Syntax;

// Lowers ordering literals to quoted markers understood by the package parser.
// Strings and comments are preserved verbatim so their contents never become values.
internal static class OrderingSyntax
{
    internal const string LessMarker = "__expressif_internal_ordering_less__";
    internal const string EqualMarker = "__expressif_internal_ordering_equal__";
    internal const string GreaterMarker = "__expressif_internal_ordering_greater__";

    public static string Normalize(string text)
    {
        if (!text.Contains("#less", StringComparison.Ordinal)
            && !text.Contains("#equal", StringComparison.Ordinal)
            && !text.Contains("#greater", StringComparison.Ordinal))
            return text;

        var result = new StringBuilder(text.Length);
        for (var index = 0; index < text.Length; index++)
        {
            var end = SkipTriviaOrString(text, index);
            if (end > index)
            {
                result.Append(text.AsSpan(index, end - index));
                index = end - 1;
                continue;
            }

            if (TryMatch(text, index, "#less", out end)
                || TryMatch(text, index, "#equal", out end)
                || TryMatch(text, index, "#greater", out end))
            {
                result.Append('"').Append(Marker(text.AsSpan(index, end - index))).Append('"');
                index = end - 1;
                continue;
            }

            result.Append(text[index]);
        }
        return result.ToString();
    }

    public static OrderingValue? Bind(string marker) => marker switch
    {
        LessMarker => OrderingValue.Less,
        EqualMarker => OrderingValue.Equal,
        GreaterMarker => OrderingValue.Greater,
        _ => null,
    };

    private static int SkipTriviaOrString(string text, int index)
    {
        if (text[index] != '`')
            return ControlFlowSyntax.SkipTriviaOrString(text, index);

        var closing = text.IndexOf('`', index + 1);
        return closing < 0 ? text.Length : closing + 1;
    }

    private static bool TryMatch(string text, int index, string literal, out int end)
    {
        end = index + literal.Length;
        return end <= text.Length
            && text.AsSpan(index, literal.Length).Equals(literal, StringComparison.Ordinal)
            && (end == text.Length || !IsNameCharacter(text[end]));
    }

    private static string Marker(ReadOnlySpan<char> literal)
        => literal.Equals("#less", StringComparison.Ordinal)
            ? LessMarker
            : literal.Equals("#equal", StringComparison.Ordinal)
                ? EqualMarker
                : GreaterMarker;

    private static bool IsNameCharacter(char value)
        => char.IsLetterOrDigit(value) || value is '-' or '_';
}
