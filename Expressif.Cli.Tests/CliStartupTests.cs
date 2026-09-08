using Expressif.Cli.Application;
using Expressif.Cli.Expressions;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class CliStartupTests
{
    [Test]
    public async Task NoArguments_Interactive_EvaluatesUntilEndOfInput()
    {
        var terminal = new TestTerminal();
        var composition = CliComposition.CreateDefault() with
        {
            Repl = () => new ReplHost(new ReplSession(new ExpressionService()), terminal),
        };
        var output = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(output);
            var exitCode = await CliInvoker.InvokeAsync([], composition, false, false);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
                Assert.That(terminal.Results, Is.EqualTo(new[] { "2", "3" }));
                Assert.That(output.ToString(), Does.Contain("Welcome to Expressif").And.Contain("Ctrl+C").And.Contain("--help"));
            });
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task NoArguments_Redirected_ShowsUsageWithoutStartingRepl(bool inputRedirected, bool outputRedirected)
        => await AssertCommandDoesNotStartRepl([], inputRedirected, outputRedirected, ExitCodes.Success, "Usage:");

    [TestCase("--help", ExitCodes.Success, "Usage:")]
    [TestCase("version", ExitCodes.Success, "")]
    [TestCase("unknown-command", ExitCodes.InvalidExpressionOrInput, "")]
    public async Task ExplicitCommand_Interactive_DoesNotStartRepl(string command, int expectedExitCode, string expectedOutput)
        => await AssertCommandDoesNotStartRepl([command], false, false, expectedExitCode, expectedOutput);

    private static async Task AssertCommandDoesNotStartRepl(
        string[] args, bool inputRedirected, bool outputRedirected, int expectedExitCode, string expectedOutput)
    {
        var composition = CliComposition.CreateDefault() with
        {
            Repl = () => throw new InvalidOperationException("The REPL must not start."),
        };
        var output = new StringWriter();
        var error = new StringWriter();
        var originalOutput = Console.Out;
        var originalError = Console.Error;
        try
        {
            Console.SetOut(output);
            Console.SetError(error);
            var exitCode = await CliInvoker.InvokeAsync(args, composition, inputRedirected, outputRedirected);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(expectedExitCode));
                Assert.That(output.ToString(), Does.Contain(expectedOutput));
                Assert.That(output.ToString(), Does.Not.Contain("Welcome to Expressif"));
            });
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }

    private sealed class TestTerminal : IReplTerminal
    {
        private readonly Queue<string?> lines = new(new string?[] { "2", "| add(1)", null });
        public List<string> Results { get; } = [];

        public string? ReadLine(string prompt, CancellationToken cancellationToken) => lines.Dequeue();
        public void WriteResult(string value) => Results.Add(value);
        public void WriteError(string message) => Assert.Fail(message);
    }
}
