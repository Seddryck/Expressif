using System.CommandLine;
using Expressif.Cli.Commands;
using Expressif.Cli.Application;

namespace Expressif.Cli;

internal static class CliRootCommandFactory
{
    public static RootCommand Create(CliServices? services = null)
    {
        services ??= CliServices.CreateDefault();
        var rootCommand = new RootCommand("Parse, bind, evaluate, run and validate Expressif expressions.");

        rootCommand.Subcommands.Add(ParseCommand.Create(services));
        rootCommand.Subcommands.Add(BindCommand.Create(services));
        rootCommand.Subcommands.Add(EvaluateCommand.Create(services));
        rootCommand.Subcommands.Add(RunCommand.Create(services));
        rootCommand.Subcommands.Add(ValidateCommand.Create(services));
        rootCommand.Subcommands.Add(HelpCommand.Create(services));
        rootCommand.Subcommands.Add(VersionCommand.Create());

        return rootCommand;
    }
}
