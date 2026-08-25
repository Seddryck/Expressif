using System.CommandLine;
using Expressif.Functions.Catalog;

namespace Expressif.Cli.Commands;

internal static class HelpCommand
{
    public static Command Create()
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
            var selectedModes = (function is null ? 0 : 1) + (list ? 1 : 0) + (scope is null ? 0 : 1);

            if (selectedModes != 1)
            {
                Console.Error.WriteLine("Specify exactly one function name, --list, or --scope.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var catalog = FunctionCatalog.Default;
            if (list)
            {
                Console.Out.WriteLine(FunctionHelpFormatter.FormatList(catalog.Functions));
                return ExitCodes.Success;
            }

            if (scope is not null)
            {
                var functions = catalog.ForScope(scope).ToArray();
                if (functions.Length == 0)
                {
                    Console.Error.WriteLine($"Unknown or empty function scope '{scope}'.");
                    return ExitCodes.InvalidExpressionOrInput;
                }

                Console.Out.WriteLine(FunctionHelpFormatter.FormatList(functions));
                return ExitCodes.Success;
            }

            var match = catalog.Find(function!);
            if (match is not null)
            {
                Console.Out.WriteLine(FunctionHelpFormatter.Format(match));
                return ExitCodes.Success;
            }

            Console.Error.WriteLine($"Unknown function '{function}'.");
            var suggestions = catalog.Suggest(function!).Select(x => x.Name).ToArray();
            if (suggestions.Length > 0)
                Console.Error.WriteLine($"Did you mean: {string.Join(", ", suggestions)}?");

            return ExitCodes.InvalidExpressionOrInput;
        });

        return command;
    }
}
