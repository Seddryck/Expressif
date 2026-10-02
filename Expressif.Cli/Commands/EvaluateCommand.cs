using System.CommandLine;
using Expressif.Cli.Configuration;
using Expressif.Cli.Application;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;
using Expressif.Serialization;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class EvaluateCommand
{
    public static Command Create(EvaluateHandler handler, IStrictUtf8TextReader textFiles, CliConfiguration? configuration = null)
    {
        var expression = new Argument<string?>("expression") { Arity = ArgumentArity.ZeroOrOne, Description = "Expression to evaluate." };
        var input = new Option<string?>("--input") { Description = "Input value passed to the expression." };
        input.Aliases.Add("-i");
        var source = new Option<string[]>("--source") { Description = "Source path or pattern. Repeat to collect multiple JSON documents." };
        source.Aliases.Add("-s");
        var collect = new Option<bool>("--collect") { Description = "Read each selected JSON document root as one element and evaluate once." };
        var scalar = new Option<bool>("--scalar") { Description = "Treat each source row as a single value. The source must contain exactly one column." };
        var sourceOptions = new Option<string[]>("--source-option") { Description = "Source-specific setting in <name>=<value> form. Repeat to add settings." };
        var file = new Option<string?>("--file") { Description = "Path to a UTF-8 file containing the expression to evaluate." };
        file.Aliases.Add("-f");
        var plan = new Option<string?>("--plan") { Description = "Path to a versioned logical-plan JSON file to evaluate." };
        var output = new Option<ValueSerializationFormat?>("--output") { Description = "Output format: raw or json." };
        var raw = new Option<bool>("--raw") { Description = "Shortcut for --output raw." };
        var outputStyle = new Option<ValueFormat?>("--output-style") { Description = "Output style: compact or pretty." };
        var pretty = new Option<bool>("--pretty") { Description = "Shortcut for --output-style pretty." };
        var compact = new Option<bool>("--compact") { Description = "Shortcut for --output-style compact." };
        var indent = new Option<string?>("--indent") { Description = "Pretty-output indentation: a space count from 0 to 8, or tab." };
        var command = new Command("evaluate", "Evaluate an Expressif expression.");
        command.Arguments.Add(expression);
        command.Options.Add(input);
        command.Options.Add(source);
        command.Options.Add(collect);
        command.Options.Add(scalar);
        command.Options.Add(sourceOptions);
        command.Options.Add(file);
        command.Options.Add(plan);
        command.Options.Add(output);
        command.Options.Add(raw);
        command.Options.Add(outputStyle);
        command.Options.Add(pretty);
        command.Options.Add(compact);
        command.Options.Add(indent);
        command.SetAction(result => Execute(result, handler, textFiles, configuration ?? CliConfiguration.CreateDefault(), expression, input, source, collect, scalar, sourceOptions,
            file, plan, output, raw, outputStyle, pretty, compact, indent));
        return command;
    }

    private static int Execute(ParseResult result, EvaluateHandler handler, IStrictUtf8TextReader textFiles, CliConfiguration configuration,
        Argument<string?> expression, Option<string?> input, Option<string[]> source, Option<bool> collect,
        Option<bool> scalar, Option<string[]> sourceOptions, Option<string?> file, Option<string?> plan,
        Option<ValueSerializationFormat?> output, Option<bool> raw,
        Option<ValueFormat?> outputStyle, Option<bool> pretty, Option<bool> compact, Option<string?> indent)
    {
        var hasInput = result.GetResult(input) is not null;
        var hasSource = result.GetResult(source) is not null;
        var sourcePaths = result.GetValue(source) ?? [];
        var collectionRequested = result.GetValue(collect);
        var optionError = ValidateOptions(result, hasInput, hasSource, sourcePaths.Length, collectionRequested,
            result.GetValue(scalar), result.GetResult(sourceOptions) is not null);
        if (optionError is not null)
        {
            Console.Error.WriteLine(optionError);
            return ExitCodes.InvalidExpressionOrInput;
        }

        if (!ExpressionCommandSourceResolver.TryResolve(
                result.GetValue(expression), result.GetValue(file), result.GetValue(plan), textFiles, out var expressionSource))
            return ExitCodes.InvalidExpressionOrInput;

        var kind = ResolveInputKind(hasInput, hasSource);
        var request = new EvaluateRequest(expressionSource, kind, result.GetValue(input), sourcePaths,
            result.GetValue(sourceOptions) ?? [], result.GetValue(scalar), collectionRequested);
        if (!ConfiguredOutput.TryResolve(configuration, "evaluate",
                result.GetValue(output), result.GetValue(raw), result.GetValue(outputStyle), result.GetValue(pretty), result.GetValue(compact), result.GetValue(indent),
                out var serializer, out var formatting, out var outputError))
            return WriteError(outputError!, ExitCodes.InvalidExpressionOrInput);
        using var observation = CliLineage.Begin(expressionSource.Text, "evaluate", hasSource ? request.SourcePaths.FirstOrDefault() : null, configuration: configuration);
        var exitCode = WriteResult(handler.Execute(request, observation.FunctionObservers), expressionSource, serializer, formatting);
        if (exitCode == ExitCodes.Success)
            observation.Complete();
        else
            observation.Fail(new InvalidOperationException("Evaluation did not complete successfully."));
        return exitCode;
    }

    private static EvaluateInputKind ResolveInputKind(bool hasInput, bool hasSource)
        => hasSource ? EvaluateInputKind.Source : hasInput ? EvaluateInputKind.Value : EvaluateInputKind.Closed;

    private static string? ValidateOptions(ParseResult result, bool hasInput, bool hasSource, int sourceCount,
        bool collect, bool scalar, bool hasSourceOptions)
    {
        if (result.Tokens.Count(token => token.Value is "--input" or "-i") > 1)
            return "The --input option can only be specified once for evaluate.";
        if (hasInput && hasSource)
            return "The --source option cannot be combined with --input.";
        if (collect && !hasSource)
            return "The --collect option requires --source.";
        if (sourceCount > 1 && !collect)
            return "The --source option can only be repeated with --collect.";
        if (collect && scalar)
            return "The --scalar option cannot be combined with --collect.";
        if (collect && hasSourceOptions)
            return "The --source-option option cannot be combined with --collect.";
        if (scalar && !hasSource)
            return "The --scalar option requires --source.";
        return hasSourceOptions && !hasSource ? "The --source-option option requires --source." : null;
    }

    private static int WriteResult(ExpressionOperationResult result, ExpressionCommandSource source,
        IValueSerializer serializer, ValueFormattingOptions formatting)
        => result switch
        {
            ExpressionSuccessResult { HasValue: true } success => WriteSuccess(success.Value, serializer, formatting),
            ExpressionValidationFailure failure => ExpressionCommandSourceResolver.WriteValidationError(failure.Exception, source),
            ExpressionInputRequiredFailure failure => WriteInputRequired(failure.Exception),
            ExpressionInputFailure failure => WriteError(failure.Message, ExitCodes.InvalidExpressionOrInput),
            ExpressionEvaluationFailure failure => WriteError(CommandErrorFormatter.FormatEvaluationError(failure.Exception), ExitCodes.EvaluationFailed, true),
            ExpressionUnexpectedFailure failure => WriteError($"Unexpected error: {failure.Exception.Message}", ExitCodes.UnexpectedInternalError),
            _ => throw new InvalidOperationException($"Unexpected evaluation result '{result.GetType().Name}'.")
        };

    private static int WriteSuccess(object? value, IValueSerializer serializer, ValueFormattingOptions formatting)
    {
        Console.Out.WriteLine(serializer.Serialize(value, formatting));
        return ExitCodes.Success;
    }

    private static int WriteInputRequired(ExpressionRequiresInputException exception)
    {
        Console.Error.WriteLine("The expression is valid, but it requires an input to be evaluated.");
        Console.Error.WriteLine(exception.Message);
        Console.Error.WriteLine("Provide an input with --input. You can load the expression with --file or a logical plan with --plan.");
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
