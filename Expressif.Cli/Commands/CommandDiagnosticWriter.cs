namespace Expressif.Cli.Commands;

internal static class CommandDiagnosticWriter
{
    private const string Red = "\u001b[31m";
    private const string Reset = "\u001b[0m";

    public static void WriteLine(string message)
        => Console.Error.WriteLine(Colorize(message, ShouldUseColor()));

    internal static string Colorize(string message, bool useColor)
        => useColor ? $"{Red}{message}{Reset}" : message;

    private static bool ShouldUseColor()
        => !Console.IsErrorRedirected
           && Environment.GetEnvironmentVariable("NO_COLOR") is null
           && !string.Equals(
               Environment.GetEnvironmentVariable("TERM"),
               "dumb",
               StringComparison.OrdinalIgnoreCase);
}
