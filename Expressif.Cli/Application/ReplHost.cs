using Expressif.Values;

namespace Expressif.Cli.Application;

internal interface IReplTerminal
{
    string? ReadLine(string prompt, CancellationToken cancellationToken);

    void WriteResult(string value);

    void WriteError(string message);
}

internal interface IReplInterruptSource
{
    IDisposable Register(Action interrupt);
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

internal sealed class ConsoleReplTerminal(
    TextReader input,
    TextWriter output,
    TextWriter error,
    IReplInterruptSource interrupts) : IReplTerminal
{
    public ConsoleReplTerminal()
        : this(Console.In, Console.Out, Console.Error, new ConsoleReplInterruptSource()) { }

    public string? ReadLine(string prompt, CancellationToken cancellationToken)
    {
        output.Write(prompt);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var registration = interrupts.Register(cancellation.Cancel);
        var read = input.ReadLineAsync();
        var interrupted = Task.Delay(Timeout.InfiniteTimeSpan, cancellation.Token);
        var completed = Task.WhenAny(read, interrupted).GetAwaiter().GetResult();
        return completed == read
            ? read.GetAwaiter().GetResult()
            : throw new OperationCanceledException(cancellation.Token);
    }

    public void WriteResult(string value) => output.WriteLine(value);

    public void WriteError(string message) => error.WriteLine(message);
}

internal sealed class ConsoleReplInterruptSource : IReplInterruptSource
{
    public IDisposable Register(Action interrupt)
    {
        ConsoleCancelEventHandler handler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            interrupt();
        };

        Console.CancelKeyPress += handler;
        return new Registration(() => Console.CancelKeyPress -= handler);
    }

    private sealed class Registration(Action unregister) : IDisposable
    {
        public void Dispose() => unregister();
    }
}
