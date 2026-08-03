using System.CommandLine;
using System.Text;

namespace Expressif.Cli.Commands;

internal static class ValidateCommand
{
    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = static (code, context) => new Expression(code, context);

    internal static Func<string, Context, ClosedExpression> BuildClosedExpression { get; set; }
        = static (code, context) => new ClosedExpression(code, context);

    public static Command Create()
    {
        var expressionArgument = new Argument<string?>("expression")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Expression to validate."
        };

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to validate."
        };
        expressionFileOption.Aliases.Add("-f");

        var openOption = new Option<bool>("--open")
        {
            Description = "Validate the expression as an open expression (default behavior)."
        };

        var closedOption = new Option<bool>("--closed")
        {
            Description = "Validate the expression as a closed expression."
        };

        var command = new Command("validate", "Validate an Expressif expression.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(expressionFileOption);
        command.Options.Add(openOption);
        command.Options.Add(closedOption);

        command.SetAction(parseResult =>
        {
            var expressionCode = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var asOpenExpression = parseResult.GetValue(openOption);
            var asClosedExpression = parseResult.GetValue(closedOption);

            if (asOpenExpression && asClosedExpression)
            {
                Console.Error.WriteLine("Options --open and --closed cannot be used together.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var useClosedValidation = asClosedExpression;

            var hasInlineExpression = !string.IsNullOrWhiteSpace(expressionCode);
            var hasExpressionFile = !string.IsNullOrWhiteSpace(expressionFilePath);

            if (hasInlineExpression && hasExpressionFile)
            {
                Console.Error.WriteLine("The expression cannot be provided both inline and through --file.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!hasInlineExpression && !hasExpressionFile)
            {
                Console.Error.WriteLine("The expression must be supplied through exactly one source: inline or --file.");
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

            try
            {
                if (useClosedValidation)
                {
                    _ = BuildClosedExpression(expressionCode, new Context());
                }
                else
                {
                    _ = BuildExpression(expressionCode, new Context());
                }

                Console.Out.WriteLine("Expression is valid.");
                return ExitCodes.Success;
            }
            catch (ExpressionRequiresInputException exception)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.InvalidExpressionOrInput;
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
