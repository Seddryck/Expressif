using System.Text.RegularExpressions;

namespace Expressif.Cli.Commands;

internal static partial class CommandErrorFormatter
{
    public static string FormatValidationError(Exception exception, string? source = null)
    {
        if (exception is NotImplementedFunctionException)
        {
            var name = TryExtractQuotedValue(exception.Message);
            return string.IsNullOrEmpty(name)
                ? exception.Message
                : $"Unknown function '{name}'.";
        }

        if (exception is Expressif.Syntax.ExpressifSyntaxException { Errors.Count: > 0 } syntaxException
            && source is not null)
        {
            var position = Math.Clamp(syntaxException.Errors[0].Span.Start, 0, source.Length);
            var line = 1;
            var column = 1;
            for (var index = 0; index < position; index++)
            {
                if (source[index] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }

            return $"{exception.Message} (line {line}, column {column}).";
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
