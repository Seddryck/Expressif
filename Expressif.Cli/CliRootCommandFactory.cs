using System.CommandLine;
using Expressif.Cli.Commands;
using Expressif.Cli.Application;

namespace Expressif.Cli;

internal static class CliRootCommandFactory
{
    public static RootCommand Create(CliComposition? composition = null)
    {
        composition ??= CliComposition.CreateDefault();
        var rootCommand = new RootCommand("Parse, bind, evaluate, run and validate Expressif expressions.");

        rootCommand.Subcommands.Add(ParseCommand.Create(composition.Parse));
        rootCommand.Subcommands.Add(BindCommand.Create(composition.Bind));
        rootCommand.Subcommands.Add(EvaluateCommand.Create(composition.Evaluate, composition.TextFiles));
        rootCommand.Subcommands.Add(RunCommand.Create(composition.Run));
        rootCommand.Subcommands.Add(ValidateCommand.Create(composition.Validate, composition.TextFiles));
        rootCommand.Subcommands.Add(HelpCommand.Create(composition.Help));
        rootCommand.Subcommands.Add(ReplCommand.Create(composition.Repl));
        rootCommand.Subcommands.Add(VersionCommand.Create());

        return rootCommand;
    }
}
