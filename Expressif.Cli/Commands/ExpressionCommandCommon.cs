using Expressif.Cli.Infrastructure;

namespace Expressif.Cli.Commands;

internal static class ExpressionCommandCommon
{
    internal static bool TryResolveExpressionCode(
        string? inlineExpression,
        string? expressionFilePath,
        IStrictUtf8TextReader textReader,
        out string expressionCode,
        out bool hasExpressionFile)
    {
        expressionCode = inlineExpression ?? string.Empty;
        hasExpressionFile = !string.IsNullOrWhiteSpace(expressionFilePath);
        var hasInlineExpression = !string.IsNullOrWhiteSpace(inlineExpression);

        if (hasInlineExpression && hasExpressionFile)
        {
            Console.Error.WriteLine("The expression cannot be provided both inline and through --file.");
            return false;
        }

        if (!hasInlineExpression && !hasExpressionFile)
        {
            Console.Error.WriteLine("The expression must be supplied through exactly one source: inline or --file.");
            return false;
        }

        if (hasExpressionFile
            && !TryReadExpressionFile(expressionFilePath!, textReader, out expressionCode))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(expressionCode))
        {
            Console.Error.WriteLine("Expression is required.");
            return false;
        }

        return true;
    }

    internal static int WriteValidationError(
        Exception exception,
        string expressionCode,
        bool hasExpressionFile,
        string? expressionFilePath)
    {
        var message = CommandErrorFormatter.FormatValidationError(exception, expressionCode);
        if (hasExpressionFile)
        {
            Console.Error.WriteLine($"The expression loaded from '{expressionFilePath}' is invalid:");
            CommandDiagnosticWriter.WriteLine(message);
            return ExitCodes.InvalidExpressionOrInput;
        }

        CommandDiagnosticWriter.WriteLine(message);
        return ExitCodes.InvalidExpressionOrInput;
    }

    private static bool TryReadExpressionFile(string path, IStrictUtf8TextReader textReader, out string expressionCode)
    {
        expressionCode = string.Empty;

        try
        {
            expressionCode = textReader.Read(path);
        }
        catch (TextFileReadException exception)
        {
            var message = exception.Kind switch
            {
                TextFileFailureKind.Directory => $"Expression file '{path}' is a directory.",
                TextFileFailureKind.NotFound => $"Expression file '{path}' was not found.",
                TextFileFailureKind.InvalidUtf8 => $"Expression file '{path}' could not be decoded as UTF-8.",
                TextFileFailureKind.Empty => $"Expression file '{path}' is empty.",
                _ => $"Expression file '{path}' could not be accessed: {exception.Message}",
            };
            Console.Error.WriteLine(message);
            return false;
        }
        return true;
    }
}
