using System.Text.Json;
using Expressif.Cli.Commands;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class BindCommandTests
{
    [TearDown]
    public void TearDown() => BindCommand.ResetDelegates();

    [Test]
    public async Task Bind_ValidExpression_DefaultsToHumanReadableTree()
    {
        var result = await InvokeAsync("bind", "trim | multiply(1.21) | round(2)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith("OpenExpression"));
            Assert.That(result.StdOut, Does.Contain("Function: trim"));
            Assert.That(result.StdOut, Does.Contain("Arg[0]: Literal = 1.21"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Bind_PredicateOnlyExpression_IsValid()
    {
        var result = await InvokeAsync("bind", "even");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.Contain("Function: even"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Bind_BinaryPredicate_PreservesBothOperands()
    {
        var result = await InvokeAsync("bind", "even |AND greater-than(5)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.Contain("Function: and"));
            Assert.That(result.StdOut, Does.Contain("Arg[0]: OpenExpression"));
            Assert.That(result.StdOut, Does.Contain("Arg[1]: OpenExpression"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Bind_FieldShorthand_ShowsBinderGeneratedFunction()
    {
        var result = await InvokeAsync("bind", ".name");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.Contain("Function: field (from FieldShorthand)"));
            Assert.That(result.StdOut, Does.Contain("Arg[0]: Literal = \"name\""));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Bind_JsonOutput_IsMachineReadable()
    {
        var result = await InvokeAsync("bind", "add(2)", "--output", "json");
        using var document = JsonDocument.Parse(result.StdOut);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(document.RootElement.GetProperty("Kind").GetString(), Is.EqualTo("OpenExpression"));
            Assert.That(document.RootElement.GetProperty("Children")[0].GetProperty("Name").GetString(), Is.EqualTo("add"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Bind_YamlOutput_IsStructured()
    {
        var result = await InvokeAsync("bind", "add(2)", "--output", "yaml");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith("kind: \"OpenExpression\""));
            Assert.That(result.StdOut, Does.Contain("name: \"add\""));
            Assert.That(result.StdOut, Does.Contain("children:"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Bind_UnknownFunction_ReturnsBindingDiagnosticAndNonZeroExitCode()
    {
        var result = await InvokeAsync("bind", "does-not-exist");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("Unknown function 'does-not-exist'."));
        });
    }

    [Test]
    public async Task Bind_InvalidSyntax_ReturnsParserDiagnosticAndNonZeroExitCode()
    {
        var result = await InvokeAsync("bind", "add(");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("Syntax error"));
        });
    }

    [Test]
    public async Task Bind_UnsupportedOutput_ReturnsClearError()
    {
        var result = await InvokeAsync("bind", "trim", "--output", "xml");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("tree, json, yaml"));
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
