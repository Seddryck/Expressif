using System.CommandLine;
using Expressif.Bindings;
using Expressif.Cli.Application;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class PlanCommand
{
    public static Command Create(PlanHandler handler)
    {
        var expressionArgument = new Argument<string>("expression")
        {
            Description = "Expression to plan."
        };
        var command = new Command("plan", "Plan an Expressif expression and display its logical tree.");
        command.Arguments.Add(expressionArgument);
        command.SetAction(parseResult =>
        {
            var expression = parseResult.GetValue(expressionArgument)!;
            try
            {
                Console.Out.WriteLine(LogicalPlanFormatter.Format(handler.Execute(expression)));
                return ExitCodes.Success;
            }
            catch (Exception exception) when (exception is ExpressifSyntaxException
                                              or BindingException
                                              or LogicalPlanningException)
            {
                return ExpressionCommandCommon.WriteValidationError(
                    exception,
                    expression,
                    hasExpressionFile: false,
                    expressionFilePath: null);
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
