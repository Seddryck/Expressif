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
        var command = CreateCommand(
            "plan",
            "Display a logical plan annotated with its discovered schemas.",
            handler,
            PlanView.Complete);
        command.Subcommands.Add(CreateCommand(
            "logical",
            "Display the canonical logical plan without schema annotations.",
            handler,
            PlanView.Logical));
        command.Subcommands.Add(CreateCommand(
            "schema",
            "Display the input, output, and node-by-node schemas of a logical plan.",
            handler,
            PlanView.Schema));
        return command;
    }

    private static Command CreateCommand(
        string name,
        string description,
        PlanHandler handler,
        PlanView view)
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
        var command = new Command(name, description);
        command.Arguments.Add(expressionArgument);
        command.Options.Add(outputOption);
        command.SetAction(parseResult => Execute(
            handler,
            view,
            parseResult.GetValue(expressionArgument)!,
            parseResult.GetValue(outputOption)!));
        return command;
    }

    private static int Execute(PlanHandler handler, PlanView view, string expression, string output)
    {
        if (!output.Equals("tree", StringComparison.OrdinalIgnoreCase)
            && !output.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Option --output must be one of: tree, json.");
            return ExitCodes.InvalidExpressionOrInput;
        }
        try
        {
            Console.Out.WriteLine(view switch
            {
                PlanView.Logical => FormatLogical(handler.Execute(expression), output),
                PlanView.Schema => FormatSchema(handler.Analyze(expression), output),
                _ => FormatComplete(handler.Analyze(expression), output),
            });
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
    }

    private static string FormatLogical(LogicalPlan plan, string output)
        => IsJson(output) ? LogicalPlanJson.Serialize(plan) : LogicalPlanFormatter.Format(plan);

    private static string FormatSchema(AnalyzedLogicalPlan analyzed, string output)
        => IsJson(output)
            ? SchemaAnalysisJson.Serialize(analyzed.Analysis)
            : SchemaAnalysisFormatter.Format(analyzed);

    private static string FormatComplete(AnalyzedLogicalPlan analyzed, string output)
        => IsJson(output)
            ? AnalyzedLogicalPlanJson.Serialize(analyzed)
            : LogicalPlanFormatter.Format(analyzed);

    private static bool IsJson(string output)
        => output.Equals("json", StringComparison.OrdinalIgnoreCase);

    private enum PlanView
    {
        Complete,
        Logical,
        Schema,
    }
}
