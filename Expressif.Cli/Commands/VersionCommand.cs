using System.CommandLine;

namespace Expressif.Cli.Commands;

internal static class VersionCommand
{
    public static Command Create()
    {
        var command = new Command("version", "Display Expressif CLI and library versions.");

        command.SetAction(_ =>
        {
            Console.Out.WriteLine($"Expressif CLI {VersionFormatter.GetVersion(typeof(VersionCommand).Assembly)}");
            Console.Out.WriteLine($"Expressif {VersionFormatter.GetVersion(typeof(Expression).Assembly)}");
            return ExitCodes.Success;
        });

        return command;
    }
}
