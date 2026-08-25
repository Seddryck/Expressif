using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli;

internal static class CliInvoker
{
    public static async Task<int> InvokeAsync(string[] args)
        => await InvokeAsync(args, CliServices.CreateDefault());

    internal static async Task<int> InvokeAsync(string[] args, CliServices services)
    {
        var rootCommand = CliRootCommandFactory.Create(services);
        var parseResult = rootCommand.Parse(args);
        return await InvokeAsync(parseResult);
    }

    internal static async Task<int> InvokeAsync(ParseResult parseResult)
    {
        var exitCode = await parseResult.InvokeAsync();
        if (parseResult.Errors.Count > 0 && exitCode != ExitCodes.Success)
            return ExitCodes.InvalidExpressionOrInput;

        return exitCode;
    }
}
