using System.Globalization;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class OutputIndentation
{
    private const string Default = "  ";

    public static bool TryResolve(string? value, ValueFormat style, out string indentation, out string? error)
    {
        indentation = Default;
        error = null;
        if (value is null)
            return true;

        if (style != ValueFormat.Pretty)
        {
            error = "The --indent option requires --output-style pretty.";
            return false;
        }

        if (value.Equals("tab", StringComparison.OrdinalIgnoreCase))
        {
            indentation = "\t";
            return true;
        }

        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var spaces)
            && spaces is >= 0 and <= 8)
        {
            indentation = new string(' ', spaces);
            return true;
        }

        error = "The --indent option must be 'tab' or an integer from 0 to 8.";
        return false;
    }
}
