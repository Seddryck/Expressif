using System.Text;
using Expressif.Bindings;

namespace Expressif.Syntax;

// Lowers branch notation to the package parser's structured argument syntax.
// Delimiters inside strings, comments, and nested expressions are never separators.
internal static class ControlFlowSyntax
{
    public static string Normalize(string text)
    {
        var result = new StringBuilder();
        for (var index = 0; index < text.Length; index++)
        {
            var end = SkipTriviaOrString(text, index);
            if (end > index)
            {
                result.Append(text[index..end]);
                index = end - 1;
                continue;
            }
            if (!char.IsLetter(text[index]) || (index > 0 && IsNameCharacter(text[index - 1])))
            {
                result.Append(text[index]);
                continue;
            }
            end = index + 1;
            while (end < text.Length && IsNameCharacter(text[end]))
                end++;
            var name = text[index..end];
            var opening = end;
            while (opening < text.Length && char.IsWhiteSpace(text[opening]))
                opening++;
            if ((name.Equals("switch", StringComparison.OrdinalIgnoreCase)
                || name.Equals("try", StringComparison.OrdinalIgnoreCase))
                && opening < text.Length && text[opening] == '(')
            {
                var closing = Closing(text, opening);
                if (text[closing] != ')')
                    throw new BindingException("A control-flow call must close with a parenthesis.");
                result.Append(name).Append('(')
                    .Append(NormalizeBranches(text[(opening + 1)..closing]))
                    .Append(')');
                index = closing;
            }
            else
            {
                result.Append(name);
                index = end - 1;
            }
        }
        return result.ToString();
    }

    private static string NormalizeBranches(string text)
    {
        var branches = Split(text, ",");
        var result = new List<string>();
        foreach (var branch in branches)
        {
            var parts = Split(branch, "=>");
            if (parts.Count != 2 || parts.Any(string.IsNullOrWhiteSpace))
                throw new BindingException("A control-flow branch requires two operands separated by '=>'.");
            result.Add(parts[0].Trim() == "_"
                ? $"fallback := {Normalize(parts[1])}"
                : $"branch({Normalize(parts[0])}, {Normalize(parts[1])})");
        }
        return string.Join(", ", result);
    }

    internal static List<string> Split(string text, string separator)
    {
        var parts = new List<string>();
        var start = 0;
        for (var index = 0; index < text.Length; index++)
        {
            var end = SkipTriviaOrString(text, index);
            if (end > index)
            {
                index = end - 1;
                continue;
            }
            if (text[index] is '(' or '[' or '{')
            {
                index = Closing(text, index);
                continue;
            }
            if (!text.AsSpan(index).StartsWith(separator, StringComparison.Ordinal))
                continue;
            parts.Add(text[start..index]);
            index += separator.Length - 1;
            start = index + 1;
        }
        parts.Add(text[start..]);
        return parts;
    }

    internal static int Closing(string text, int opening)
    {
        var depth = 1;
        for (var index = opening + 1; index < text.Length; index++)
        {
            var end = SkipTriviaOrString(text, index);
            if (end > index)
            {
                index = end - 1;
                continue;
            }
            if (text[index] is '(' or '[' or '{')
            {
                depth++;
            }
            else if (text[index] is ')' or ']' or '}')
            {
                if (--depth == 0)
                    return index;
            }
        }
        throw new BindingException("Unclosed control-flow expression.");
    }

    internal static int SkipTriviaOrString(string text, int index)
    {
        if (text[index] is '\"' or '\'')
        {
            var quote = text[index];
            for (var end = index + 1; end < text.Length; end++)
            {
                if (text[end] == '\\')
                    end++;
                else if (text[end] == quote)
                    return end + 1;
            }
            return text.Length;
        }
        if (text.AsSpan(index).StartsWith("//"))
        {
            var end = text.IndexOf('\n', index);
            return end < 0 ? text.Length : end;
        }
        if (text.AsSpan(index).StartsWith("/*"))
        {
            var end = text.IndexOf("*/", index + 2, StringComparison.Ordinal);
            return end < 0 ? text.Length : end + 2;
        }
        return index;
    }

    private static bool IsNameCharacter(char value)
        => char.IsLetterOrDigit(value) || value is '-' or '_' or '.' or '@' or ':';
}
