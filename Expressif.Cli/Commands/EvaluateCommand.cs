using System.CommandLine;

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

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to evaluate."
        };
        expressionFileOption.Aliases.Add("-f");

        var command = new Command("evaluate", "Evaluate an Expressif expression.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(inputOption);
        command.Options.Add(expressionFileOption);

        command.SetAction(parseResult =>
        {
            var inlineExpression = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var input = parseResult.GetValue(inputOption);
            var hasInputOption = parseResult.GetResult(inputOption) is not null;

            if (!ExpressionCommandCommon.TryResolveExpressionCode(
                    inlineExpression,
                    expressionFilePath,
                    out var expressionCode,
                    out var hasExpressionFile))
            {
                return ExitCodes.InvalidExpressionOrInput;
            }

            var useClosedEvaluation = !hasInputOption;

            if (useClosedEvaluation)
            {
                ClosedExpression closedExpression;
                try
                {
                    closedExpression = BuildClosedExpression(expressionCode, new Context());
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
                    Console.Out.WriteLine(result ?? "null");
                    return ExitCodes.Success;
                }
                catch (ExpressionRequiresInputException exception)
                {
                    Console.Error.WriteLine(exception.Message);
                    return ExitCodes.InvalidExpressionOrInput;
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    Console.Error.WriteLine(exception.Message);
                    return ExitCodes.EvaluationFailed;
                }
            }

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
}
