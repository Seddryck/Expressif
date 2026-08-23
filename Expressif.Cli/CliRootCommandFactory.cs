using System.CommandLine;
using Expressif.Cli.Commands;

namespace Expressif.Cli;

internal static class CliRootCommandFactory
{
    public static RootCommand Create()
    {
        var rootCommand = new RootCommand("Parse, evaluate, run and validate Expressif expressions.");

        rootCommand.Subcommands.Add(ParseCommand.Create());
        rootCommand.Subcommands.Add(EvaluateCommand.Create());
        rootCommand.Subcommands.Add(RunCommand.Create());
        rootCommand.Subcommands.Add(ValidateCommand.Create());
        rootCommand.Subcommands.Add(VersionCommand.Create());

        return rootCommand;
    }
}
