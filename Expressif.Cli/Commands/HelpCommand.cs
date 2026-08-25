using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class HelpCommand
{
    public static Command Create(HelpHandler handler)
    {
        var functionArgument = new Argument<string?>("function")
        {
            Description = "Canonical function name or alias.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        var listOption = new Option<bool>("--list")
        {
            Description = "List all public functions.",
        };
        var scopeOption = new Option<string?>("--scope")
        {
            Description = "List public functions in a scope.",
        };

        var command = new Command("help", "Display documentation for Expressif functions.");
        command.Arguments.Add(functionArgument);
        command.Options.Add(listOption);
        command.Options.Add(scopeOption);
        command.SetAction(parseResult =>
        {
            var function = parseResult.GetValue(functionArgument);
            var list = parseResult.GetValue(listOption);
            var scope = parseResult.GetValue(scopeOption);
            if (!HelpRequest.TryCreate(function, list, scope, out var request))
            {
                Console.Error.WriteLine("Specify exactly one function name, --list, or --scope.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            try
            {
                return WriteResult(handler.Execute(request!));
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unexpected error: {exception.Message}");
                return ExitCodes.UnexpectedInternalError;
            }
        });

        return command;
    }

    private static int WriteResult(HelpResult result)
    {
        switch (result)
        {
            case FunctionHelpResult function:
                Console.Out.WriteLine(FunctionHelpFormatter.Format(function.Function));
                return ExitCodes.Success;
            case FunctionListHelpResult list:
                Console.Out.WriteLine(FunctionHelpFormatter.FormatList(list.Functions));
                return ExitCodes.Success;
            case UnknownScopeHelpResult scope:
                Console.Error.WriteLine($"Unknown or empty function scope '{scope.Scope}'.");
                return ExitCodes.InvalidExpressionOrInput;
            case UnknownFunctionHelpResult function:
                Console.Error.WriteLine($"Unknown function '{function.Name}'.");
                if (function.Suggestions.Count > 0)
                    Console.Error.WriteLine($"Did you mean: {string.Join(", ", function.Suggestions)}?");
                return ExitCodes.InvalidExpressionOrInput;
            default:
                throw new InvalidOperationException($"Unknown help result '{result.GetType().Name}'.");
        }
    }
}
