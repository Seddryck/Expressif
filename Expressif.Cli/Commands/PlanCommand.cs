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
        var outputOption = new Option<string>("--output")
        {
            Description = "Output representation: tree or json.",
            DefaultValueFactory = static _ => "tree"
        };
        var command = new Command("plan", "Plan an Expressif expression and display its logical tree.");
        command.Arguments.Add(expressionArgument);
        command.Options.Add(outputOption);
        command.SetAction(parseResult =>
        {
            var expression = parseResult.GetValue(expressionArgument)!;
            var output = parseResult.GetValue(outputOption)!;
            if (!output.Equals("tree", StringComparison.OrdinalIgnoreCase)
                && !output.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("Option --output must be one of: tree, json.");
                return ExitCodes.InvalidExpressionOrInput;
            }
            try
            {
                var plan = handler.Execute(expression);
                Console.Out.WriteLine(output.Equals("json", StringComparison.OrdinalIgnoreCase)
                    ? LogicalPlanJson.Serialize(plan)
                    : LogicalPlanFormatter.Format(plan));
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
