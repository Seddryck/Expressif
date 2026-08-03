using Expressif.Cli.Commands;
using System.CommandLine;

namespace Expressif.Cli;

internal static class CliRootCommandFactory
{
    public static RootCommand Create()
    {
        var rootCommand = new RootCommand("Evaluate and validate Expressif expressions.");

        rootCommand.Subcommands.Add(EvaluateCommand.Create());
        rootCommand.Subcommands.Add(RunCommand.Create());
        rootCommand.Subcommands.Add(ValidateCommand.Create());
        rootCommand.Subcommands.Add(VersionCommand.Create());

        return rootCommand;
    }
}
