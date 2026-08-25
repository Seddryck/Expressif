using System.CommandLine;
using Expressif.Cli.Application;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class EvaluateCommand
{
    public static Command Create(EvaluateHandler handler, IStrictUtf8TextReader textFiles)
    {
        var expression = new Argument<string?>("expression") { Arity = ArgumentArity.ZeroOrOne, Description = "Expression to evaluate." };
        var input = new Option<string?>("--input") { Description = "Input value passed to the expression." };
        input.Aliases.Add("-i");
        var source = new Option<string?>("--source") { Description = "Path to a source whose complete row set is passed as one array." };
        source.Aliases.Add("-s");
        var scalar = new Option<bool>("--scalar") { Description = "Treat each source row as a single value. The source must contain exactly one column." };
        var sourceOptions = new Option<string[]>("--source-option") { Description = "Source-specific setting in <name>=<value> form. Repeat to add settings." };
        var file = new Option<string?>("--file") { Description = "Path to a UTF-8 file containing the expression to evaluate." };
        file.Aliases.Add("-f");
        var command = new Command("evaluate", "Evaluate an Expressif expression.");
        command.Arguments.Add(expression);
        command.Options.Add(input);
        command.Options.Add(source);
        command.Options.Add(scalar);
        command.Options.Add(sourceOptions);
        command.Options.Add(file);
        command.SetAction(result => Execute(result, handler, textFiles, expression, input, source, scalar, sourceOptions, file));
        return command;
    }

    private static int Execute(ParseResult result, EvaluateHandler handler, IStrictUtf8TextReader textFiles,
        Argument<string?> expression, Option<string?> input, Option<string?> source,
        Option<bool> scalar, Option<string[]> sourceOptions, Option<string?> file)
    {
        var hasInput = result.GetResult(input) is not null;
        var hasSource = result.GetResult(source) is not null;
        var optionError = ValidateOptions(result, hasInput, hasSource, result.GetValue(scalar), result.GetResult(sourceOptions) is not null);
        if (optionError is not null)
        {
            Console.Error.WriteLine(optionError);
            return ExitCodes.InvalidExpressionOrInput;
        }

        var filePath = result.GetValue(file);
        if (!ExpressionCommandCommon.TryResolveExpressionCode(
                result.GetValue(expression), filePath, textFiles, out var code, out var fromFile))
            return ExitCodes.InvalidExpressionOrInput;

        var kind = hasSource ? EvaluateInputKind.Source : hasInput ? EvaluateInputKind.Value : EvaluateInputKind.Closed;
        var request = new EvaluateRequest(code, kind, result.GetValue(input), result.GetValue(source),
            result.GetValue(sourceOptions) ?? [], result.GetValue(scalar));
        return WriteResult(handler.Execute(request), code, fromFile, filePath);
    }

    private static string? ValidateOptions(ParseResult result, bool hasInput, bool hasSource, bool scalar, bool hasSourceOptions)
    {
        if (result.Tokens.Count(token => token.Value is "--input" or "-i") > 1)
            return "The --input option can only be specified once for evaluate.";
        if (hasInput && hasSource)
            return "The --source option cannot be combined with --input.";
        if (scalar && !hasSource)
            return "The --scalar option requires --source.";
        return hasSourceOptions && !hasSource ? "The --source-option option requires --source." : null;
    }

    private static int WriteResult(ExpressionOperationResult result, string code, bool fromFile, string? filePath)
        => result switch
        {
            ExpressionSuccessResult { HasValue: true } success => WriteSuccess(success.Value),
            ExpressionValidationFailure failure => ExpressionCommandCommon.WriteValidationError(failure.Exception, code, fromFile, filePath),
            ExpressionInputRequiredFailure failure => WriteInputRequired(failure.Exception),
            ExpressionInputFailure failure => WriteError(failure.Message, ExitCodes.InvalidExpressionOrInput),
            ExpressionEvaluationFailure failure => WriteError(CommandErrorFormatter.FormatEvaluationError(failure.Exception), ExitCodes.EvaluationFailed, true),
            ExpressionUnexpectedFailure failure => WriteError($"Unexpected error: {failure.Exception.Message}", ExitCodes.UnexpectedInternalError),
            _ => throw new InvalidOperationException($"Unexpected evaluation result '{result.GetType().Name}'.")
        };

    private static int WriteSuccess(object? value)
    {
        Console.Out.WriteLine(ValueFormatter.Format(value));
        return ExitCodes.Success;
    }

    private static int WriteInputRequired(ExpressionRequiresInputException exception)
    {
        Console.Error.WriteLine("The expression is valid, but it requires an input to be evaluated.");
        Console.Error.WriteLine(exception.Message);
        Console.Error.WriteLine("Provide an input with --input. You can load the expression from a file with --file.");
        return ExitCodes.InvalidExpressionOrInput;
    }

    private static int WriteError(string message, int exitCode, bool diagnostic = false)
    {
        if (diagnostic)
            CommandDiagnosticWriter.WriteLine(message);
        else
            Console.Error.WriteLine(message);
        return exitCode;
    }
}
