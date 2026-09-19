using System.Text;

namespace Expressif.Values;

internal static class InlineValueWriter
{
    public static bool TryWrite(
        StringBuilder builder,
        object? value,
        ValueFormattingOptions options,
        Func<string> formatCompact)
    {
        if (options.Format != ValueFormat.Pretty
            || value is null
            || !options.InlineValueTypes.Contains(value.GetType()))
        {
            return false;
        }

        var compact = formatCompact();
        if (compact.Contains('\r') || compact.Contains('\n')
            || compact.Length > options.PreferredLineWidth - CurrentLineLength(builder))
        {
            return false;
        }

        builder.Append(compact);
        return true;
    }

    private static int CurrentLineLength(StringBuilder builder)
    {
        for (var index = builder.Length - 1; index >= 0; index--)
        {
            if (builder[index] == '\n')
                return builder.Length - index - 1;
        }

        return builder.Length;
    }
}
