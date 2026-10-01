using System.CommandLine;
using Expressif.Bindings;
using Expressif.Cli.Application;
using Expressif.Cli.Infrastructure;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class PlanCommand
{
    public static Command Create(PlanHandler handler, IStrictUtf8TextReader textReader)
    {
        var command = CreateExpressionCommand(
            "plan",
            "Display a logical plan annotated with its discovered schemas.",
            handler,
            PlanView.Complete);
        command.Subcommands.Add(CreateExpressionCommand(
            "logical",
            "Display the canonical logical plan without schema annotations.",
            handler,
            PlanView.Logical));
        command.Subcommands.Add(CreateSchemaCommand(handler, textReader));
        return command;
    }

    private static Command CreateExpressionCommand(
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

    private static Command CreateSchemaCommand(PlanHandler handler, IStrictUtf8TextReader textReader)
    {
        var expressionArgument = new Argument<string?>("expression")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Expression to analyze."
        };
        var logicalOption = new Option<string?>("--logical")
        {
            Description = "Logical-plan JSON file to analyze."
        };
        var stdinOption = new Option<bool>("--stdin")
        {
            Description = "Read logical-plan JSON from standard input."
        };
        var outputOption = CreateOutputOption();
        var command = new Command(
            "schema",
            "Display the input, output, and node-by-node schemas of a logical plan.");
        command.Arguments.Add(expressionArgument);
        command.Options.Add(logicalOption);
        command.Options.Add(stdinOption);
        command.Options.Add(outputOption);
        command.SetAction(parseResult => ExecuteSchema(
            handler,
            textReader,
            parseResult.GetValue(expressionArgument),
            parseResult.GetValue(logicalOption),
            parseResult.GetValue(stdinOption),
            parseResult.GetValue(outputOption)!));
        return command;
    }

    private static Option<string> CreateOutputOption()
        => new("--output")
        {
            Description = "Output representation: tree or json.",
            DefaultValueFactory = static _ => "tree"
        };

    private static int Execute(PlanHandler handler, PlanView view, string expression, string output)
    {
        if (!ValidateOutput(output))
            return ExitCodes.InvalidExpressionOrInput;
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

    private static int ExecuteSchema(
        PlanHandler handler,
        IStrictUtf8TextReader textReader,
        string? expression,
        string? logical,
        bool stdin,
        string output)
    {
        if (!ValidateOutput(output))
            return ExitCodes.InvalidExpressionOrInput;

        var sourceCount = (string.IsNullOrWhiteSpace(expression) ? 0 : 1)
            + (string.IsNullOrWhiteSpace(logical) ? 0 : 1)
            + (stdin ? 1 : 0);
        if (sourceCount != 1)
        {
            Console.Error.WriteLine(
                "Plan schema requires exactly one source: an expression, --logical <filename>, or --stdin.");
            return ExitCodes.InvalidExpressionOrInput;
        }

        if (!string.IsNullOrWhiteSpace(expression))
            return Execute(handler, PlanView.Schema, expression, output);

        if (!TryReadLogicalPlan(logical, stdin, textReader, handler, out var plan))
            return ExitCodes.InvalidExpressionOrInput;

        Console.Out.WriteLine(FormatSchema(handler.Analyze(plan!), output));
        return ExitCodes.Success;
    }

    private static bool TryReadLogicalPlan(
        string? filename,
        bool stdin,
        IStrictUtf8TextReader textReader,
        PlanHandler handler,
        out LogicalPlan? plan)
    {
        plan = null;
        string json;
        if (stdin)
        {
            json = Console.In.ReadToEnd();
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.Error.WriteLine("Standard input does not contain a logical plan.");
                return false;
            }
        }
        else
        {
            try
            {
                json = textReader.Read(filename!);
            }
            catch (TextFileReadException exception)
            {
                Console.Error.WriteLine(FormatLogicalPlanFileError(filename!, exception));
                return false;
            }
        }

        try
        {
            plan = handler.Import(json);
            return true;
        }
        catch (LogicalPlanFormatException exception)
        {
            Console.Error.WriteLine($"The logical plan is invalid: {exception.Message}");
            return false;
        }
    }

    private static string FormatLogicalPlanFileError(string path, TextFileReadException exception)
        => exception.Kind switch
        {
            TextFileFailureKind.Directory => $"Logical-plan file '{path}' is a directory.",
            TextFileFailureKind.NotFound => $"Logical-plan file '{path}' was not found.",
            TextFileFailureKind.InvalidUtf8 => $"Logical-plan file '{path}' could not be decoded as UTF-8.",
            TextFileFailureKind.Empty => $"Logical-plan file '{path}' is empty.",
            _ => $"Logical-plan file '{path}' could not be accessed: {exception.Message}",
        };

    private static bool ValidateOutput(string output)
    {
        if (output.Equals("tree", StringComparison.OrdinalIgnoreCase)
            || output.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        Console.Error.WriteLine("Option --output must be one of: tree, json.");
        return false;
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
