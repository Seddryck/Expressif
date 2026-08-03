using Sprache;
using System.Text.RegularExpressions;

namespace Expressif.Cli.Commands;

internal static partial class CommandErrorFormatter
{
    public static string FormatValidationError(Exception exception)
    {
        if (exception is NotImplementedFunctionException)
        {
            var name = TryExtractQuotedValue(exception.Message);
            return string.IsNullOrEmpty(name)
                ? exception.Message
                : $"Unknown function '{name}'.";
        }

        return exception.Message;
    }

    public static int WriteValidationError(Exception exception)
    {
        Console.Error.WriteLine(FormatValidationError(exception));
        return ExitCodes.InvalidExpressionOrInput;
    }

    private static string? TryExtractQuotedValue(string message)
    {
        var match = QuotedValueRegex().Match(message);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex("'([^']+)'")]
    private static partial Regex QuotedValueRegex();
}
