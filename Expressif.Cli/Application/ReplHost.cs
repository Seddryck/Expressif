using Expressif.Values;

namespace Expressif.Cli.Application;

internal interface IReplTerminal
{
    string? ReadLine(string prompt, CancellationToken cancellationToken);

    void WriteResult(string value);

    void WriteError(string message);
}

internal sealed class ReplHost(ReplSession session, IReplTerminal terminal)
{
    public int Run(CancellationToken cancellationToken = default)
    {
        try
        {
            while (terminal.ReadLine("> ", cancellationToken) is { } source)
            {
                var result = session.Execute(source);
                if (result is ReplEvaluationResult evaluation)
                    terminal.WriteResult(ValueFormatter.Format(evaluation.Value));
                else if (result is ReplErrorResult error)
                    terminal.WriteError(error.Message);
            }
        }
        catch (OperationCanceledException)
        {
            // Ctrl+C ends the current REPL cleanly.
        }

        return ExitCodes.Success;
    }
}

internal sealed class ConsoleReplTerminal : IReplTerminal
{
    public string? ReadLine(string prompt, CancellationToken cancellationToken)
    {
        Console.Out.Write(prompt);
        return Console.In.ReadLineAsync(cancellationToken).AsTask().GetAwaiter().GetResult();
    }

    public void WriteResult(string value) => Console.Out.WriteLine(value);

    public void WriteError(string message) => Console.Error.WriteLine(message);
}
