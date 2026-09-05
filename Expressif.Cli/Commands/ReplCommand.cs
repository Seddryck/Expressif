using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class ReplCommand
{
    public static Command Create(Func<ReplHost> hostFactory)
    {
        var command = new Command("repl", "Start an interactive Expressif session.");
        command.SetAction(_ => Execute(hostFactory));
        return command;
    }

    private static int Execute(Func<ReplHost> hostFactory)
    {
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        Console.CancelKeyPress += handler;
        try
        {
            return hostFactory().Run(cancellation.Token);
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }
}
