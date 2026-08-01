using System.CommandLine;

namespace Expressif.Cli;

internal static class CliInvoker
{
    public static async Task<int> InvokeAsync(string[] args)
    {
        var rootCommand = CliRootCommandFactory.Create();
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
