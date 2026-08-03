using Expressif.Cli.Commands;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class CliCommandTests
{
    private readonly List<string> tempFilesToDelete = [];

    [TearDown]
    public void TearDown()
    {
        EvaluateCommand.BuildExpression = static (code, context) => new Expression(code, context);
        ValidateCommand.BuildExpression = static (code, context) => new Expression(code, context);

        foreach (var path in tempFilesToDelete)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        tempFilesToDelete.Clear();
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
    public async Task Evaluate_ExpressionFile_ProducesSameResultAsInlineExpression()
    {
        var path = CreateTempFile("trim\n| upper\n");

        var result = await InvokeAsync("evaluate", "--file", path, "--input", "  nikola tesla  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("NIKOLA TESLA"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_ExpressionFile_WithUtf8Bom_IsSupported()
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-bom-{Guid.NewGuid():N}.expr");
        File.WriteAllText(path, "trim | upper", new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        try
        {
            var result = await InvokeAsync("evaluate", "--file", path, "--input", "nikola");

            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
                Assert.That(result.StdOut.Trim(), Is.EqualTo("NIKOLA"));
                Assert.That(result.StdErr, Is.Empty);
            });
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public async Task Evaluate_ExpressionFile_RelativePath_IsResolvedFromCurrentDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"expressif-cwd-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        var originalDirectory = Directory.GetCurrentDirectory();
        try
        {
            var expressionDirectory = Path.Combine(tempDirectory, "expressions");
            Directory.CreateDirectory(expressionDirectory);

            var expressionPath = Path.Combine(expressionDirectory, "transform.expr");
            File.WriteAllText(expressionPath, "trim | upper");

            Directory.SetCurrentDirectory(tempDirectory);

            var result = await InvokeAsync("evaluate", "--file", Path.Combine("expressions", "transform.expr"), "--input", "nikola");

            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
                Assert.That(result.StdOut.Trim(), Is.EqualTo("NIKOLA"));
                Assert.That(result.StdErr, Is.Empty);
            });
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Evaluate_RunAlias_LoadsExpressionFile()
    {
        var path = CreateTempFile("trim | upper");

        var result = await InvokeAsync("run", "--file", path, "--input", "nikola");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("NIKOLA"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_BothInlineAndExpressionFile_ReturnsClearError()
    {
        var path = CreateTempFile("trim | upper");

        var result = await InvokeAsync("evaluate", "name | upper", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression cannot be provided both inline and through --expression-file."));
        });
    }

    [Test]
    public async Task Evaluate_MissingInlineAndExpressionFile_ReturnsClearError()
    {
        var result = await InvokeAsync("evaluate");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression must be supplied through exactly one source: inline or --expression-file."));
        });
    }

    [Test]
    public async Task Evaluate_ExpressionFile_NotFound_ReturnsClearError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-missing-{Guid.NewGuid():N}.expr");
        var result = await InvokeAsync("evaluate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo($"Expression file '{path}' was not found."));
        });
    }

    [Test]
    public async Task Evaluate_ExpressionFile_IsDirectory_ReturnsClearError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-dir-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);

        try
        {
            var result = await InvokeAsync("evaluate", "--file", path);

            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
                Assert.That(result.StdOut, Is.Empty);
                Assert.That(result.StdErr.Trim(), Is.EqualTo($"Expression file '{path}' is a directory."));
            });
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
    }

    [Test]
    public async Task Evaluate_ExpressionFile_Empty_ReturnsClearError()
    {
        var path = CreateTempFile(" \r\n\t ");

        var result = await InvokeAsync("evaluate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo($"Expression file '{path}' is empty."));
        });
    }

    [Test]
    public async Task Evaluate_ExpressionFile_InvalidUtf8_ReturnsClearError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-invalid-utf8-{Guid.NewGuid():N}.expr");
        File.WriteAllBytes(path, [0xC3, 0x28]);

        try
        {
            var result = await InvokeAsync("evaluate", "--file", path);

            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
                Assert.That(result.StdOut, Is.Empty);
                Assert.That(result.StdErr.Trim(), Is.EqualTo($"Expression file '{path}' could not be decoded as UTF-8."));
            });
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public async Task Evaluate_ExpressionFile_InvalidExpression_ReturnsSourceAwareError()
    {
        var path = CreateTempFile("trim | upper)");

        var result = await InvokeAsync("evaluate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain($"The expression loaded from '{path}' is invalid:"));
            Assert.That(result.StdErr, Does.Match("(?i)line"));
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

    private string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-{Guid.NewGuid():N}.expr");
        File.WriteAllText(path, content, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        tempFilesToDelete.Add(path);
        return path;
    }

    private sealed record InvocationResult(int ExitCode, string StdOut, string StdErr);
}
