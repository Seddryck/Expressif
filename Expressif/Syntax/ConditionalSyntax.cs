using System.Text;
using Expressif.Bindings;

namespace Expressif.Syntax;

internal static class ConditionalSyntax
{
    public static string Normalize(string text)
    {
        if (!text.Contains("?>", StringComparison.Ordinal) && !text.Contains("<?", StringComparison.Ordinal))
            return text;
        // Rewrite nested operands first, keeping all literal and comment text intact.
        var nested = new StringBuilder();
        for (var index = 0; index < text.Length; index++)
        {
            var end = ControlFlowSyntax.SkipTriviaOrString(text, index);
            if (end > index)
            {
                nested.Append(text[index..end]);
                index = end - 1;
            }
            else if (text[index] is '(' or '[' or '{')
            {
                var closing = ControlFlowSyntax.Closing(text, index);
                nested.Append(text[index]).Append(Normalize(text[(index + 1)..closing])).Append(text[closing]);
                index = closing;
            }
            else
            {
                nested.Append(text[index]);
            }
        }
        return NormalizeSeparated(nested.ToString(), 0);
    }

    private static string NormalizeSeparated(string text, int level)
    {
        string[] separators = [",", "=>", ":=", "|"];
        if (level == separators.Length)
            return NormalizeStage(text);
        var separator = separators[level];
        return string.Join(separator, ControlFlowSyntax.Split(text, separator)
            .Select(part => NormalizeSeparated(part, level + 1)));
    }

    private static string NormalizeStage(string text)
    {
        var forward = ControlFlowSyntax.Split(text, "?>");
        var backward = ControlFlowSyntax.Split(text, "<?");
        if (forward.Count == 1 && backward.Count == 1)
            return text;
        if (forward.Count + backward.Count != 3)
            throw new BindingException("Chained conditional operators require explicit parentheses.");
        var parts = forward.Count == 2 ? forward : backward;
        if (parts.Any(string.IsNullOrWhiteSpace))
            throw new BindingException("A conditional operator requires two operands.");
        var name = forward.Count == 2 ? "conditional-forward" : "conditional-backward";
        return $"{name}({parts[0]}, {parts[1]}\n)";
    }
}
