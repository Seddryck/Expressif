using System.Text.RegularExpressions;
using Expressif.Syntax;

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

        if (exception is ExpressifSyntaxException syntaxException)
            return FormatSyntaxErrors(syntaxException, source);

        if (exception is Expressif.Bindings.BindingException)
            return $"Binding error [EXPR2001]:{Environment.NewLine}{exception.Message}";

        return exception.Message;
    }

    public static string FormatEvaluationError(Exception exception, int? inputRow = null)
    {
        var message = $"Evaluation error [EXPR3001]:{Environment.NewLine}{exception.Message}";
        return inputRow is null
            ? message
            : $"{message}{Environment.NewLine}{Environment.NewLine}Input row: {inputRow}";
    }

    public static string FormatRuntimeError(Exception exception, int? inputRow = null)
    {
        var message = $"Runtime error [EXPR4001]:{Environment.NewLine}{exception.Message}";
        return inputRow is null
            ? message
            : $"{message}{Environment.NewLine}{Environment.NewLine}Input row: {inputRow}";
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

    private static string FormatSyntaxErrors(ExpressifSyntaxException exception, string? source)
    {
        if (exception.Errors.Count == 0 || source is null)
            return $"Syntax error [EXPR1001]:{Environment.NewLine}{exception.Message}";

        return string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            exception.Errors.Select(error => FormatSyntaxError(error, source)));
    }

    private static string FormatSyntaxError(SyntaxError error, string source)
    {
        var start = Math.Clamp(error.Span.Start, 0, source.Length);
        var (line, column, lineStart, lineEnd) = Locate(source, start);
        var sourceLine = source[lineStart..lineEnd].TrimEnd('\r');
        var offset = Math.Clamp(start - lineStart, 0, sourceLine.Length);
        var availableLength = Math.Max(1, sourceLine.Length - offset);
        var markerLength = Math.Clamp(error.Span.Length, 1, availableLength);
        var marker = new string(' ', offset) + new string('^', markerLength);
        var description = error.IsMissing
            ? $"Missing {DescribeNode(error)}."
            : $"Unexpected {DescribeNode(error)}.";

        return $"Syntax error [EXPR1001] at line {line}, column {column}:{Environment.NewLine}" +
               $"  {sourceLine}{Environment.NewLine}" +
               $"  {marker}{Environment.NewLine}" +
               description;
    }

    private static string DescribeNode(SyntaxError error)
    {
        if (!string.IsNullOrWhiteSpace(error.Text))
            return $"'{error.Text.Replace("'", "''", StringComparison.Ordinal)}'";

        return string.IsNullOrWhiteSpace(error.NodeType)
            ? "syntax"
            : error.NodeType.Replace('_', ' ');
    }

    private static (int Line, int Column, int LineStart, int LineEnd) Locate(string source, int position)
    {
        var line = 1;
        var lineStart = 0;
        for (var index = 0; index < position; index++)
        {
            if (source[index] != '\n')
                continue;

            line++;
            lineStart = index + 1;
        }

        var lineEnd = source.IndexOf('\n', position);
        if (lineEnd < 0)
            lineEnd = source.Length;

        return (line, position - lineStart + 1, lineStart, lineEnd);
    }
}
