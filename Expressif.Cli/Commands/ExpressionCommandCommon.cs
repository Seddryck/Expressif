using System.Text;

namespace Expressif.Cli.Commands;

internal static class ExpressionCommandCommon
{
    internal static bool TryResolveExpressionCode(
        string? inlineExpression,
        string? expressionFilePath,
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
            && !TryReadExpressionFile(expressionFilePath!, out expressionCode))
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

    internal static int WriteValidationError(Exception exception, bool hasExpressionFile, string? expressionFilePath)
    {
        if (hasExpressionFile)
        {
            Console.Error.WriteLine($"The expression loaded from '{expressionFilePath}' is invalid:");
            Console.Error.WriteLine(CommandErrorFormatter.FormatValidationError(exception));
            return ExitCodes.InvalidExpressionOrInput;
        }

        return CommandErrorFormatter.WriteValidationError(exception);
    }

    private static bool TryReadExpressionFile(string path, out string expressionCode)
    {
        expressionCode = string.Empty;

        if (Directory.Exists(path))
        {
            Console.Error.WriteLine($"Expression file '{path}' is a directory.");
            return false;
        }

        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Expression file '{path}' was not found.");
            return false;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), detectEncodingFromByteOrderMarks: true);
            expressionCode = reader.ReadToEnd();
        }
        catch (DecoderFallbackException)
        {
            Console.Error.WriteLine($"Expression file '{path}' could not be decoded as UTF-8.");
            return false;
        }
        catch (Exception exception) when (exception is IOException
                                          or UnauthorizedAccessException
                                          or ArgumentException
                                          or NotSupportedException)
        {
            Console.Error.WriteLine($"Expression file '{path}' could not be accessed: {exception.Message}");
            return false;
        }

        if (string.IsNullOrWhiteSpace(expressionCode))
        {
            Console.Error.WriteLine($"Expression file '{path}' is empty.");
            return false;
        }

        return true;
    }
}
