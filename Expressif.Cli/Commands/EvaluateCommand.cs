using System.CommandLine;

namespace Expressif.Cli.Commands;

internal static class EvaluateCommand
{
    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = static (code, context) => new Expression(code, context);

    public static Command Create()
    {
        var expressionArgument = new Argument<string>("expression")
        {
            Description = "Expression to evaluate."
        };

        var inputOption = new Option<string?>("--input")
        {
            Description = "Input value passed to the expression."
        };
        inputOption.Aliases.Add("-i");

        var command = new Command("evaluate", "Evaluate an Expressif expression.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(inputOption);

        command.SetAction(parseResult =>
        {
            var expressionCode = parseResult.GetValue(expressionArgument);
            var input = parseResult.GetValue(inputOption);

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
}
