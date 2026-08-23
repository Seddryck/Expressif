using System.Text.Json;
using Expressif.Cli.Commands;
using Expressif.Syntax;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class ParseCommandTests
{
    [TearDown]
    public void TearDown() => ParseCommand.ResetDelegates();

    [Test]
    public async Task Parse_ValidExpression_DefaultsToHumanReadableTree()
    {
        var result = await InvokeAsync("parse", "trim | multiply(1.21) | round(2)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith("OpenExpression"));
            Assert.That(result.StdOut, Does.Contain("FunctionCall: trim"));
            Assert.That(result.StdOut, Does.Contain("NumericLiteral: 1.21"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Parse_JsonOutput_IsMachineReadable()
    {
        var result = await InvokeAsync("parse", "add(2)", "--output", "json");
        using var document = JsonDocument.Parse(result.StdOut);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(document.RootElement.GetProperty("Kind").GetString(), Is.EqualTo("OpenExpression"));
            Assert.That(document.RootElement.GetProperty("Children").GetArrayLength(), Is.GreaterThan(0));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Parse_YamlOutput_IsStructured()
    {
        var result = await InvokeAsync("parse", "add(2)", "--output", "yaml");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith("kind: \"OpenExpression\""));
            Assert.That(result.StdOut, Does.Contain("children:"));
            Assert.That(result.StdOut, Does.Contain("- kind: \"FunctionCall\""));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Parse_InvalidSyntax_ReturnsDiagnosticAndNonZeroExitCode()
    {
        var result = await InvokeAsync("parse", "add(");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("Syntax error"));
        });
    }

    [Test]
    public async Task Parse_SyntaxErrorAfterTab_PreservesTabInCaretMarker()
    {
        var result = await InvokeAsync("parse", "trim\t@");
        var lines = result.StdErr.Split(Environment.NewLine);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(lines[1], Is.EqualTo("  trim\t@"));
            Assert.That(lines[2], Is.EqualTo("      \t^"));
        });
    }

    [Test]
    public void FormatTree_QuotedLiteralEndingInWhitespace_PreservesTrailingWhitespace()
    {
        var syntax = ExpressionParser.Parse("\"value \t\"");

        var result = SyntaxTreeFormatter.Format(syntax, "tree");

        Assert.That(result, Does.EndWith("QuotedLiteral: value \t"));
    }

    [Test]
    public async Task Parse_UnsupportedOutput_ReturnsClearError()
    {
        var result = await InvokeAsync("parse", "trim", "--output", "xml");

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
