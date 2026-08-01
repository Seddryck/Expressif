using Sprache;
using System.Text.RegularExpressions;

namespace Expressif.Cli.Commands;

internal static partial class CommandErrorFormatter
{
    public static int WriteValidationError(Exception exception)
    {
        if (exception is ParseException)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitCodes.InvalidExpressionOrInput;
        }

        if (exception is NotImplementedFunctionException)
        {
            var name = TryExtractQuotedValue(exception.Message);
            if (string.IsNullOrEmpty(name))
                Console.Error.WriteLine(exception.Message);
            else
                Console.Error.WriteLine($"Unknown function '{name}'.");

            return ExitCodes.InvalidExpressionOrInput;
        }

        if (exception is MissingOrUnexpectedParametersFunctionException)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitCodes.InvalidExpressionOrInput;
        }

        Console.Error.WriteLine(exception.Message);
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
