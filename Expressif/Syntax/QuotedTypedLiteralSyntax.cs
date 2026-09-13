using System.Text;

namespace Expressif.Syntax;

internal static class QuotedTypedLiteralSyntax
{
    internal const string Marker = "__expressif_internal_quoted_typed_literal__";
    internal const string MarkerEnd = "__expressif_internal_quoted_typed_literal_end__";

    public static string Normalize(string text)
    {
        var result = new StringBuilder(text.Length);
        var containers = new Stack<(char Closing, bool IsInterval)>();
        var quoted = false;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '"' && (index == 0 || text[index - 1] != '\\'))
                quoted = !quoted;

            if (!quoted)
                UpdateContainers(text, index, containers);

            if (quoted || IsIntervalBound(containers) || text[index] != '#'
                || index + 1 >= text.Length || text[index + 1] != '"')
            {
                result.Append(text[index]);
                continue;
            }

            var closing = FindClosingQuote(text, index + 1);
            if (closing < 0)
            {
                result.Append(text[index]);
                continue;
            }

            var typeStart = closing + 1;
            var typeEnd = typeStart;
            if (typeStart < text.Length && text[typeStart] == ':')
            {
                typeEnd++;
                while (typeEnd < text.Length && IsTypeNameCharacter(text[typeEnd]))
                    typeEnd++;
            }

            var representation = text[(index + 2)..closing];
            var typeName = typeEnd > typeStart ? text[(typeStart + 1)..typeEnd] : string.Empty;
            result.Append("T(\"").Append(Marker).Append("\", \"").Append(MarkerEnd)
                .Append("\", \"").Append(representation).Append("\", \"").Append(typeName).Append("\")");
            index = typeEnd > typeStart ? typeEnd - 1 : closing;
        }
        return result.ToString();
    }

    private static int FindClosingQuote(string text, int opening)
    {
        for (var index = opening + 1; index < text.Length; index++)
        {
            if (text[index] == '"' && !IsEscaped(text, index))
                return index;
        }
        return -1;
    }

    private static bool IsEscaped(string text, int index)
    {
        var slashes = 0;
        for (var current = index - 1; current >= 0 && text[current] == '\\'; current--)
            slashes++;
        return slashes % 2 != 0;
    }

    private static bool IsTypeNameCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character is '-' or '_';

    private static bool IsIntervalBound(Stack<(char Closing, bool IsInterval)> containers)
        => containers.TryPeek(out var container) && container.IsInterval;

    private static void UpdateContainers(
        string text,
        int index,
        Stack<(char Closing, bool IsInterval)> containers)
    {
        var current = text[index];
        if (current is '(' or '[' or '{')
        {
            var interval = current is '(' or '[' && index > 0 && text[index - 1] == 'I'
                && (index == 1 || !char.IsAsciiLetterOrDigit(text[index - 2]));
            containers.Push((current switch { '(' => ')', '[' => ']', _ => '}' }, interval));
        }
        else if (containers.TryPeek(out var container) && current == container.Closing)
        {
            containers.Pop();
        }
    }
}
