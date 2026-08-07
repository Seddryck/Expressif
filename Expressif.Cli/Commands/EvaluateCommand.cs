using System.CommandLine;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class EvaluateCommand
{
    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = static (code, context) => new Expression(code, context);

    internal static Func<string, Context, ClosedExpression> BuildClosedExpression { get; set; }
        = static (code, context) => new ClosedExpression(code, context);

    internal static Func<ClosedExpression, object?> EvaluateClosedExpression { get; set; }
        = static expression => expression.Evaluate();

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

        var sourceOption = new Option<string?>("--source")
        {
            Description = "Path to a source whose complete row set is passed as one array."
        };
        sourceOption.Aliases.Add("-s");

        var scalarOption = new Option<bool>("--scalar")
        {
            Description = "Treat each source row as a single value. The source must contain exactly one column."
        };

        var sourceProfileOption = new Option<string[]>("--source-option")
        {
            Description = "Source-specific setting in <name>=<value> form. Repeat to add settings."
        };

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to evaluate."
        };
        expressionFileOption.Aliases.Add("-f");

        var command = new Command("evaluate", "Evaluate an Expressif expression.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(inputOption);
        command.Options.Add(sourceOption);
        command.Options.Add(scalarOption);
        command.Options.Add(sourceProfileOption);
        command.Options.Add(expressionFileOption);

        command.SetAction(parseResult =>
        {
            var inlineExpression = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var input = parseResult.GetValue(inputOption);
            var sourcePath = parseResult.GetValue(sourceOption);
            var scalar = parseResult.GetValue(scalarOption);
            var sourceProfileOptions = parseResult.GetValue(sourceProfileOption) ?? [];
            var hasInputOption = parseResult.GetResult(inputOption) is not null;
            var hasSourceOption = parseResult.GetResult(sourceOption) is not null;
            var hasSourceProfileOption = parseResult.GetResult(sourceProfileOption) is not null;
            var inputOptionOccurrences = parseResult.Tokens.Count(token => token.Value is "--input" or "-i");

            if (inputOptionOccurrences > 1)
            {
                Console.Error.WriteLine("The --input option can only be specified once for evaluate.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (hasInputOption && hasSourceOption)
            {
                Console.Error.WriteLine("The --source option cannot be combined with --input.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (scalar && !hasSourceOption)
            {
                Console.Error.WriteLine("The --scalar option requires --source.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (hasSourceProfileOption && !hasSourceOption)
            {
                Console.Error.WriteLine("The --source-option option requires --source.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!ExpressionCommandCommon.TryResolveExpressionCode(
                    inlineExpression,
                    expressionFilePath,
                    out var expressionCode,
                    out var hasExpressionFile))
            {
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (hasSourceOption)
                return EvaluateSource(expressionCode, sourcePath, scalar, sourceProfileOptions, hasExpressionFile, expressionFilePath);

            if (!hasInputOption)
                return EvaluateClosed(expressionCode, hasExpressionFile, expressionFilePath);

            return EvaluateOpen(expressionCode, input, hasExpressionFile, expressionFilePath);
        });

        return command;
    }

    private static int EvaluateSource(
        string expressionCode,
        string? sourcePath,
        bool scalar,
        IReadOnlyList<string> sourceOptions,
        bool hasExpressionFile,
        string? expressionFilePath)
    {
        object?[] sourceRows;
        try
        {
            sourceRows = RunCommand.BuildSourceRows(sourcePath, sourceOptions, scalar).ToArray();
        }
        catch (FormatException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitCodes.InvalidExpressionOrInput;
        }

        return EvaluateOpen(expressionCode, sourceRows, hasExpressionFile, expressionFilePath);
    }

    private static int EvaluateClosed(string expressionCode, bool hasExpressionFile, string? expressionFilePath)
    {
        ClosedExpression closedExpression;
        try
        {
            closedExpression = BuildClosedExpression(expressionCode, new Context());
        }
        catch (ExpressionRequiresInputException exception)
        {
            var openValidationResult = ValidateAsOpenExpression(expressionCode, hasExpressionFile, expressionFilePath);
            if (openValidationResult != ExitCodes.Success)
                return openValidationResult;

            Console.Error.WriteLine("The expression is valid, but it requires an input to be evaluated.");
            Console.Error.WriteLine(exception.Message);
            Console.Error.WriteLine("Provide an input with --input. You can load the expression from a file with --file.");
            return ExitCodes.InvalidExpressionOrInput;
        }
        catch (Exception exception) when (exception is Sprache.ParseException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException)
        {
            return ExpressionCommandCommon.WriteValidationError(exception, hasExpressionFile, expressionFilePath);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Unexpected error: {exception.Message}");
            return ExitCodes.UnexpectedInternalError;
        }

        try
        {
            var result = EvaluateClosedExpression(closedExpression);
            Console.Out.WriteLine(ValueFormatter.Format(result));
            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitCodes.EvaluationFailed;
        }
    }

    private static int ValidateAsOpenExpression(string expressionCode, bool hasExpressionFile, string? expressionFilePath)
    {
        try
        {
            _ = BuildExpression(expressionCode, new Context());
            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is Sprache.ParseException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException)
        {
            return ExpressionCommandCommon.WriteValidationError(exception, hasExpressionFile, expressionFilePath);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Unexpected error: {exception.Message}");
            return ExitCodes.UnexpectedInternalError;
        }
    }

    private static int EvaluateOpen(string expressionCode, object? input, bool hasExpressionFile, string? expressionFilePath)
    {
        Expressif.Expression openExpression;
            try
            {
                openExpression = BuildExpression(expressionCode, new Context());
            }
            catch (Exception exception) when (exception is Sprache.ParseException
                                              or NotImplementedFunctionException
                                              or MissingOrUnexpectedParametersFunctionException)
            {
                return ExpressionCommandCommon.WriteValidationError(exception, hasExpressionFile, expressionFilePath);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unexpected error: {exception.Message}");
                return ExitCodes.UnexpectedInternalError;
            }

            try
            {
                var result = openExpression.Evaluate(input);
                Console.Out.WriteLine(ValueFormatter.Format(result));
                return ExitCodes.Success;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.EvaluationFailed;
            }
    }
}
