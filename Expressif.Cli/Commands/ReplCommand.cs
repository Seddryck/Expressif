using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class ReplCommand
{
    public static Command Create(Func<ReplHost> hostFactory)
    {
        var command = new Command("repl", "Start an interactive Expressif session.");
        command.SetAction(_ => hostFactory().Run());
        return command;
    }
}
