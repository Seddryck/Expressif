using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class OutputStyleSelection
{
    public static bool TryResolve(
        ValueFormat? outputStyle,
        bool pretty,
        bool compact,
        out ValueFormat style,
        out string? error)
    {
        var selectionCount = (outputStyle.HasValue ? 1 : 0) + (pretty ? 1 : 0) + (compact ? 1 : 0);
        if (selectionCount > 1)
        {
            style = default;
            error = "The --output-style, --pretty, and --compact options are mutually exclusive.";
            return false;
        }

        style = pretty ? ValueFormat.Pretty : outputStyle ?? ValueFormat.Compact;
        error = null;
        return true;
    }
}
