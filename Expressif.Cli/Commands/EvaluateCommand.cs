using System.CommandLine;
using Expressif.Values;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class EvaluateCommand
{
    public static Command Create(CliServices services)
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

        command.SetAction(parseResult => Execute(
            parseResult,
            services,
            expressionArgument,
            inputOption,
            sourceOption,
            scalarOption,
            sourceProfileOption,
            expressionFileOption));

        return command;
    }

    private static int Execute(
        ParseResult parseResult,
        CliServices services,
        Argument<string?> expressionArgument,
        Option<string?> inputOption,
        Option<string?> sourceOption,
        Option<bool> scalarOption,
        Option<string[]> sourceProfileOption,
        Option<string?> expressionFileOption)
    {
        var hasInputOption = parseResult.GetResult(inputOption) is not null;
        var hasSourceOption = parseResult.GetResult(sourceOption) is not null;
        var scalar = parseResult.GetValue(scalarOption);
        var hasSourceProfileOption = parseResult.GetResult(sourceProfileOption) is not null;

        var optionValidationResult = ValidateOptions(
            parseResult, hasInputOption, hasSourceOption, scalar, hasSourceProfileOption);
        if (optionValidationResult != ExitCodes.Success)
            return optionValidationResult;

        var inlineExpression = parseResult.GetValue(expressionArgument);
        var expressionFilePath = parseResult.GetValue(expressionFileOption);
        if (!ExpressionCommandCommon.TryResolveExpressionCode(
                inlineExpression,
                expressionFilePath,
                services.TextFiles,
                out var expressionCode,
                out var hasExpressionFile))
        {
            return ExitCodes.InvalidExpressionOrInput;
        }

        if (hasSourceOption)
        {
            var sourcePath = parseResult.GetValue(sourceOption);
            var sourceProfileOptions = parseResult.GetValue(sourceProfileOption) ?? [];
            return EvaluateSource(services, expressionCode, sourcePath, scalar, sourceProfileOptions, hasExpressionFile, expressionFilePath);
        }

        return hasInputOption
            ? EvaluateInput(services, expressionCode, parseResult.GetValue(inputOption), hasExpressionFile, expressionFilePath)
            : EvaluateClosed(services, expressionCode, hasExpressionFile, expressionFilePath);
    }

    private static int ValidateOptions(
        ParseResult parseResult,
        bool hasInputOption,
        bool hasSourceOption,
        bool scalar,
        bool hasSourceProfileOption)
    {
        if (parseResult.Tokens.Count(token => token.Value is "--input" or "-i") > 1)
            return WriteOptionError("The --input option can only be specified once for evaluate.");

        if (hasInputOption && hasSourceOption)
            return WriteOptionError("The --source option cannot be combined with --input.");

        if (scalar && !hasSourceOption)
            return WriteOptionError("The --scalar option requires --source.");

        return hasSourceProfileOption && !hasSourceOption
            ? WriteOptionError("The --source-option option requires --source.")
            : ExitCodes.Success;
    }

    private static int WriteOptionError(string message)
    {
        Console.Error.WriteLine(message);
        return ExitCodes.InvalidExpressionOrInput;
    }

    private static int EvaluateSource(CliServices services,
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
            sourceRows = new RunHandler(services).BuildSourceRows(sourcePath, sourceOptions, scalar).ToArray();
        }
        catch (FormatException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitCodes.InvalidExpressionOrInput;
        }

        return EvaluateOpen(services, expressionCode, sourceRows, hasExpressionFile, expressionFilePath);
    }

    private static int EvaluateClosed(CliServices services, string expressionCode, bool hasExpressionFile, string? expressionFilePath)
    {
        IExpression closedExpression;
        try
        {
            closedExpression = services.Expressions.CompileClosed(expressionCode, new Context());
        }
        catch (ExpressionRequiresInputException exception)
        {
            var openValidationResult = ValidateAsOpenExpression(services, expressionCode, hasExpressionFile, expressionFilePath);
            if (openValidationResult != ExitCodes.Success)
                return openValidationResult;

            Console.Error.WriteLine("The expression is valid, but it requires an input to be evaluated.");
            Console.Error.WriteLine(exception.Message);
            Console.Error.WriteLine("Provide an input with --input. You can load the expression from a file with --file.");
            return ExitCodes.InvalidExpressionOrInput;
        }
        catch (Exception exception) when (exception is Expressif.Syntax.ExpressifSyntaxException
                                          or Expressif.Bindings.BindingException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException)
        {
            return ExpressionCommandCommon.WriteValidationError(exception, expressionCode, hasExpressionFile, expressionFilePath);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Unexpected error: {exception.Message}");
            return ExitCodes.UnexpectedInternalError;
        }

        try
        {
            var result = services.Expressions.Evaluate(closedExpression, null);
            Console.Out.WriteLine(ValueFormatter.Format(result));
            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            CommandDiagnosticWriter.WriteLine(CommandErrorFormatter.FormatEvaluationError(exception));
            return ExitCodes.EvaluationFailed;
        }
    }

    private static int EvaluateInput(CliServices services,
        string expressionCode,
        string? input,
        bool hasExpressionFile,
        string? expressionFilePath)
    {
        object? parsedInput;
        try
        {
            parsedInput = services.Values.Parse(input ?? string.Empty);
        }
        catch (FormatException exception)
        {
            Console.Error.WriteLine($"Invalid input syntax for --input '{input}': {exception.Message}");
            return ExitCodes.InvalidExpressionOrInput;
        }

        return EvaluateOpen(services, expressionCode, parsedInput, hasExpressionFile, expressionFilePath);
    }

    private static int ValidateAsOpenExpression(CliServices services, string expressionCode, bool hasExpressionFile, string? expressionFilePath)
    {
        try
        {
            _ = services.Expressions.CompileOpen(expressionCode, new Context());
            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is Expressif.Syntax.ExpressifSyntaxException
                                          or Expressif.Bindings.BindingException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException)
        {
            return ExpressionCommandCommon.WriteValidationError(exception, expressionCode, hasExpressionFile, expressionFilePath);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Unexpected error: {exception.Message}");
            return ExitCodes.UnexpectedInternalError;
        }
    }

    private static int EvaluateOpen(CliServices services, string expressionCode, object? input, bool hasExpressionFile, string? expressionFilePath)
    {
        Expressif.IExpression openExpression;
        try
        {
            openExpression = services.Expressions.CompileOpen(expressionCode, new Context());
        }
        catch (Exception exception) when (exception is Expressif.Syntax.ExpressifSyntaxException
                                          or Expressif.Bindings.BindingException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException)
        {
            return ExpressionCommandCommon.WriteValidationError(exception, expressionCode, hasExpressionFile, expressionFilePath);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Unexpected error: {exception.Message}");
            return ExitCodes.UnexpectedInternalError;
        }

        try
        {
            var result = services.Expressions.Evaluate(openExpression, input);
            Console.Out.WriteLine(ValueFormatter.Format(result));
            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            CommandDiagnosticWriter.WriteLine(CommandErrorFormatter.FormatEvaluationError(exception));
            return ExitCodes.EvaluationFailed;
        }
    }
}
