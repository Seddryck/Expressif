using System.CommandLine;
using Expressif.Cli.Configuration;

namespace Expressif.Cli.Commands;

internal static class ConfigCommand
{
    public static Command Create(CliConfiguration configuration)
    {
        var command = new Command("config", "Read and update persistent CLI preferences.");
        foreach (var operation in new[] { "get", "set", "unset" })
        {
            var subcommand = new Command(operation, $"{operation} a configuration setting.");
            var key = new Argument<string>("key");
            var value = new Argument<string>("value");
            subcommand.Arguments.Add(key);
            if (operation == "set")
                subcommand.Arguments.Add(value);
            subcommand.SetAction(result => Execute(() =>
            {
                if (operation == "set")
                    configuration.Set(result.GetValue(key)!, result.GetValue(value)!);
                else if (operation == "unset")
                    configuration.Unset(result.GetValue(key)!);
                else
                    Console.Out.WriteLine(configuration.Get(result.GetValue(key)!));
            }));
            command.Subcommands.Add(subcommand);
        }

        var path = new Command("path", "Show the configuration file path.");
        path.SetAction(_ => Console.Out.WriteLine(configuration.Path));
        command.Subcommands.Add(path);
        var list = new Command("list", "Show effective configuration values.");
        var scope = new Option<string?>("--command") { Description = "Resolve settings for repl, run, or evaluate." };
        list.Options.Add(scope);
        list.SetAction(result => Execute(() =>
        {
            var selected = result.GetValue(scope);
            foreach (var setting in new[] { "output-style", "indent" })
            {
                var key = selected is null ? setting : selected + "." + setting;
                Console.Out.WriteLine($"{key}={configuration.Get(key)} (source: {configuration.GetSource(key)})");
            }
        }));
        command.Subcommands.Add(list);
        return command;
    }

    private static int Execute(Action action)
    {
        try
        {
            action();
            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is FormatException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Configuration error: {exception.Message}");
            return ExitCodes.InvalidExpressionOrInput;
        }
    }
}
