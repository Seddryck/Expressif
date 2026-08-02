using System.CommandLine;

namespace Expressif.Cli.Commands;

internal static class ValidateCommand
{
    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = static (code, context) => new Expression(code, context);

    public static Command Create()
    {
        var expressionArgument = new Argument<string>("expression")
        {
            Description = "Expression to validate."
        };

        var command = new Command("validate", "Validate an Expressif expression.");

        command.Arguments.Add(expressionArgument);

        command.SetAction(parseResult =>
        {
            var expressionCode = parseResult.GetValue(expressionArgument);

            if (string.IsNullOrWhiteSpace(expressionCode))
            {
                Console.Error.WriteLine("Expression is required.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            try
            {
                _ = BuildExpression(expressionCode, new Context());
                Console.Out.WriteLine("Expression is valid.");
                return ExitCodes.Success;
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
        });

        return command;
    }
}
