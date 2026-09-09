using Expressif.Syntax;

namespace Expressif.Semantics;

internal static class FieldSpans
{
    // Canonical selectors currently carry names/indices, but no individual spans.
    // Locate separators only inside this already-parsed reference, respecting
    // quoted selectors. This does not parse expressions or infer scope semantics.
    public static SourceSpan Get(RecordAccessSyntax access, int selection)
    {
        var starts = new List<int> { 0 };
        var quote = '\0';
        var firstSeparator = true;
        for (var index = 0; index < access.Text.Length; index++)
        {
            var character = access.Text[index];
            if (quote != '\0')
            {
                if (character == '\\')
                    index++;
                else if (character == quote)
                    quote = '\0';
                continue;
            }
            if (character is '\"' or '\'')
            {
                quote = character;
            }
            else if (character == '.')
            {
                if (firstSeparator)
                    firstSeparator = false;
                else
                    starts.Add(index);
            }
        }
        if (starts.Count != access.Fields.Count)
            return access.Span;
        var start = starts[selection];
        var end = selection + 1 < starts.Count ? starts[selection + 1] : access.Text.Length;
        return new(access.Span.Start + start, end - start);
    }
}
