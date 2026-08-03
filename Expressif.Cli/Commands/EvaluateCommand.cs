using System.CommandLine;
using System.Text;

namespace Expressif.Cli.Commands;

internal static class EvaluateCommand
{
    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = static (code, context) => new Expression(code, context);

    public static Command Create()
    {
        var expressionArgument = new Argument<string?>("expression")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Expression to evaluate."
        };

        var inputOption = new Option<string?>("--input")
        {
            Description = "Input value passed to the expression."
        };
        inputOption.Aliases.Add("-i");

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to evaluate."
        };
        expressionFileOption.Aliases.Add("-f");
        expressionFileOption.Aliases.Add("--expression-file");

        var command = new Command("evaluate", "Evaluate an Expressif expression.");
        command.Aliases.Add("run");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(inputOption);
        command.Options.Add(expressionFileOption);

        command.SetAction(parseResult =>
        {
            var expressionCode = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var input = parseResult.GetValue(inputOption);

            var hasInlineExpression = !string.IsNullOrWhiteSpace(expressionCode);
            var hasExpressionFile = !string.IsNullOrWhiteSpace(expressionFilePath);

            if (hasInlineExpression && hasExpressionFile)
            {
                Console.Error.WriteLine("The expression cannot be provided both inline and through --expression-file.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!hasInlineExpression && !hasExpressionFile)
            {
                Console.Error.WriteLine("The expression must be supplied through exactly one source: inline or --expression-file.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (hasExpressionFile
                && !TryReadExpressionFile(expressionFilePath!, out expressionCode))
            {
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (string.IsNullOrWhiteSpace(expressionCode))
            {
                Console.Error.WriteLine("Expression is required.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            Expressif.Expression expression;
            try
            {
                expression = BuildExpression(expressionCode, new Context());
            }
            catch (Exception exception) when (exception is Sprache.ParseException
                                              or NotImplementedFunctionException
                                              or MissingOrUnexpectedParametersFunctionException)
            {
                if (hasExpressionFile)
                {
                    Console.Error.WriteLine($"The expression loaded from '{expressionFilePath}' is invalid:");
                    Console.Error.WriteLine(CommandErrorFormatter.FormatValidationError(exception));
                    return ExitCodes.InvalidExpressionOrInput;
                }

                return CommandErrorFormatter.WriteValidationError(exception);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unexpected error: {exception.Message}");
                return ExitCodes.UnexpectedInternalError;
            }

            try
            {
                var result = expression.Evaluate(input);
                Console.Out.WriteLine(result ?? "null");
                return ExitCodes.Success;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.EvaluationFailed;
            }
        });

        return command;
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
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
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
