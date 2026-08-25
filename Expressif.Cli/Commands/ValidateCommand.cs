using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class ValidateCommand
{
    public static Command Create(CliServices services)
    {
        var expression = new Argument<string?>("expression") { Arity = ArgumentArity.ZeroOrOne, Description = "Expression to validate." };
        var file = new Option<string?>("--file") { Description = "Path to a UTF-8 file containing the expression to validate." };
        file.Aliases.Add("-f");
        var open = new Option<bool>("--open") { Description = "Validate the expression as an open expression (default behavior)." };
        var closed = new Option<bool>("--closed") { Description = "Validate the expression as a closed expression." };
        var handler = new ValidateHandler(services.Expressions);
        var command = new Command("validate", "Validate an Expressif expression.");
        command.Arguments.Add(expression);
        command.Options.Add(file);
        command.Options.Add(open);
        command.Options.Add(closed);
        command.SetAction(result =>
        {
            if (result.GetValue(open) && result.GetValue(closed))
            {
                Console.Error.WriteLine("Options --open and --closed cannot be used together.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var filePath = result.GetValue(file);
            if (!ExpressionCommandCommon.TryResolveExpressionCode(
                    result.GetValue(expression), filePath, services.TextFiles, out var code, out var fromFile))
                return ExitCodes.InvalidExpressionOrInput;

            return WriteResult(handler.Execute(new ValidateRequest(code, result.GetValue(closed))), code, fromFile, filePath);
        });
        return command;
    }

    private static int WriteResult(ExpressionOperationResult result, string code, bool fromFile, string? filePath)
        => result switch
        {
            ExpressionSuccessResult => WriteSuccess(),
            ExpressionInputRequiredFailure failure => WriteError(failure.Exception.Message, ExitCodes.InvalidExpressionOrInput),
            ExpressionValidationFailure failure => ExpressionCommandCommon.WriteValidationError(failure.Exception, code, fromFile, filePath),
            ExpressionUnexpectedFailure failure => WriteError($"Unexpected error: {failure.Exception.Message}", ExitCodes.UnexpectedInternalError),
            _ => throw new InvalidOperationException($"Unexpected validation result '{result.GetType().Name}'.")
        };

    private static int WriteSuccess()
    {
        Console.Out.WriteLine("Expression is valid.");
        return ExitCodes.Success;
    }

    private static int WriteError(string message, int exitCode)
    {
        Console.Error.WriteLine(message);
        return exitCode;
    }
}
