using Expressif.Cli.Commands;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class CliCommandTests
{
    [TearDown]
    public void TearDown()
    {
        EvaluateCommand.BuildExpression = static (code, context) => new Expression(code, context);
        ValidateCommand.BuildExpression = static (code, context) => new Expression(code, context);
    }

    [Test]
    public async Task Evaluate_ValidExpression_ReturnsResultAndSuccessCode()
    {
        var result = await InvokeAsync("evaluate", "absolute | add(5)", "--input", "-12");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("17"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_NullInput_ReturnsExpectedResult()
    {
        var result = await InvokeAsync("evaluate", "null-to-empty | count-chars");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("0"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_NullResult_WritesLiteralNull()
    {
        var result = await InvokeAsync("evaluate", "{} | first");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("null"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_EvaluationFailure_ReturnsEvaluationExitCode()
    {
        var result = await InvokeAsync("evaluate", "add(1)", "--input", "abc");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Is.Not.Empty);
        });
    }

    [Test]
    public async Task Validate_ValidExpression_ReturnsSuccessAndMessage()
    {
        var result = await InvokeAsync("validate", "absolute | add(5)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("Expression is valid."));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Validate_InvalidExpression_ReturnsValidationExitCode()
    {
        var result = await InvokeAsync("validate", "absolute | unknown(5)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unknown function 'unknown'."));
        });
    }

    [Test]
    public async Task Version_ReturnsBothVersions()
    {
        var result = await InvokeAsync("version");
        var lines = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(lines, Has.Length.EqualTo(2));
            Assert.That(lines[0], Does.StartWith("Expressif CLI "));
            Assert.That(lines[1], Does.StartWith("Expressif "));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Root_HelpOutput_ListsCommands()
    {
        var result = await InvokeAsync("--help");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.Contain("evaluate"));
            Assert.That(result.StdOut, Does.Contain("validate"));
            Assert.That(result.StdOut, Does.Contain("version"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Validate_MissingRequiredExpression_ReturnsInvalidInputExitCode()
    {
        var result = await InvokeAsync("validate");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdErr, Is.Not.Empty);
        });
    }

    [Test]
    public async Task InvalidCommand_ReturnsInvalidInputExitCode()
    {
        var result = await InvokeAsync("invalid-command");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdErr, Is.Not.Empty);
        });
    }

    [Test]
    public async Task Validate_UnexpectedException_ReturnsUnexpectedInternalErrorExitCode()
    {
        ValidateCommand.BuildExpression = static (_, _) => throw new InvalidOperationException("boom");

        var result = await InvokeAsync("validate", "absolute");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.UnexpectedInternalError));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unexpected error: boom"));
        });
    }

    private static async Task<InvocationResult> InvokeAsync(params string[] args)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;

        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        Console.SetOut(stdout);
        Console.SetError(stderr);

        try
        {
            var exitCode = await CliInvoker.InvokeAsync(args);
            return new InvocationResult(exitCode, stdout.ToString(), stderr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private sealed record InvocationResult(int ExitCode, string StdOut, string StdErr);
}
