using Expressif.Cli.Commands;
using System.Data;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class CliCommandTests
{
    private readonly List<string> tempFilesToDelete = [];

    [TearDown]
    public void TearDown()
    {
        EvaluateCommand.BuildExpression = static (code, context) => new Expression(code, context);
        EvaluateCommand.BuildClosedExpression = static (code, context) => new ClosedExpression(code, context);
        EvaluateCommand.EvaluateClosedExpression = static expression => expression.Evaluate();
        RunCommand.ResetDelegates();
        ValidateCommand.BuildExpression = static (code, context) => new Expression(code, context);
        ValidateCommand.BuildClosedExpression = static (code, context) => new ClosedExpression(code, context);

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
    public async Task Evaluate_MultipleInputOptions_ReturnsClearError()
    {
        var result = await InvokeAsync("evaluate", "absolute | add(5)", "--input", "-12", "--input", "-5");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdErr, Does.Contain("--input"));
            Assert.That(result.StdErr, Does.Contain("single argument"));
        });
    }

    [Test]
    public async Task Evaluate_EmptyStringInput_ReturnsExpectedResult()
    {
        var result = await InvokeAsync("evaluate", "null-to-empty | count-chars", "--input", string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("0"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_ImplicitSum_WithStringArrayLiteralInput_ReturnsAggregatedResult()
    {
        var result = await InvokeAsync("evaluate", "sum", "--input", "{1,2,2}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("5"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_MapFieldAddSum_RecordArrayLiteralInput_ReturnsExpectedValue()
    {
        var result = await InvokeAsync(
            "evaluate",
            "map(field(value) | add(1)) | sum",
            "--input",
            "{{value:=1}, {value:=2}, {value:=3}}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("9"));
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
    public async Task Evaluate_ClosedExpressionWithoutInput_EvaluatesOnce()
    {
        var result = await InvokeAsync("evaluate", "5 | add(3)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("8"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_ClosedExpressionFromFileWithoutInput_EvaluatesOnce()
    {
        var path = CreateTempFile("5\n| add(3)\n| multiply(2)");

        var result = await InvokeAsync("evaluate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("16"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_OpenExpressionWithoutInput_ReturnsInputRequiredError()
    {
        var result = await InvokeAsync("evaluate", "upper");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("The expression is valid, but it requires an input to be evaluated."));
            Assert.That(result.StdErr, Does.Contain("The expression cannot be evaluated without an input because it references 'upper'."));
            Assert.That(result.StdErr, Does.Contain("Provide an input with --input. You can load the expression from a file with --file."));
        });
    }

    [Test]
    public async Task Evaluate_ClosedEvaluationInputRequired_ButInvalidOpenExpression_ReturnsValidationError()
    {
        EvaluateCommand.BuildClosedExpression = static (_, _) => throw new ExpressionRequiresInputException("upper");
        EvaluateCommand.BuildExpression = static (_, _) => throw new NotImplementedFunctionException("unknown");

        var result = await InvokeAsync("evaluate", "upper");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Not.Contain("The expression is valid, but it requires an input to be evaluated."));
            Assert.That(result.StdErr, Does.Contain("unknown"));
        });
    }

    [Test]
    public async Task Evaluate_ClosedExpression_UnexpectedCompileException_ReturnsUnexpectedInternalErrorExitCode()
    {
        EvaluateCommand.BuildClosedExpression = static (_, _) => throw new InvalidOperationException("boom closed compile");

        var result = await InvokeAsync("evaluate", "5 | add(3)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.UnexpectedInternalError));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unexpected error: boom closed compile"));
        });
    }

    [Test]
    public async Task Evaluate_OpenExpression_UnexpectedCompileException_ReturnsUnexpectedInternalErrorExitCode()
    {
        EvaluateCommand.BuildExpression = static (_, _) => throw new InvalidOperationException("boom open compile");

        var result = await InvokeAsync("evaluate", "trim | upper", "--input", "abc");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.UnexpectedInternalError));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unexpected error: boom open compile"));
        });
    }

    [Test]
    public async Task Evaluate_ClosedExpression_RuntimeFailure_ReturnsEvaluationExitCode()
    {
        EvaluateCommand.EvaluateClosedExpression = static _ => throw new InvalidOperationException("boom closed runtime");

        var result = await InvokeAsync("evaluate", "5 | add(3)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("boom closed runtime"));
        });
    }

    [Test]
    public async Task Evaluate_ExplicitEmptyInput_UsesInputBasedEvaluation()
    {
        var result = await InvokeAsync("evaluate", "trim | upper", "--input", string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("(empty)"));
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
    public async Task Evaluate_BothInlineAndExpressionFile_ReturnsClearError()
    {
        var path = CreateTempFile("trim | upper");

        var result = await InvokeAsync("evaluate", "name | upper", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression cannot be provided both inline and through --file."));
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
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression must be supplied through exactly one source: inline or --file."));
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
    public async Task Evaluate_ExpressionFile_AccessFailure_ReturnsClearError()
    {
        var path = CreateTempFile("trim | upper");
        using var lockStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var result = await InvokeAsync("evaluate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.StartWith($"Expression file '{path}' could not be accessed:"));
        });
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
    public async Task Run_BatchEnumerable_EvaluatesEachDirectElementAndReturnsSuccess()
    {
        var result = await InvokeAsync("run", "absolute", "--batch", "{1, -2, 3}");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_BatchNestedEnumerable_EnumeratesOnlyOuterInput()
    {
        var result = await InvokeAsync("run", "count", "--batch", "{{1, 2, 3}, {4, 5}}");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "3", "2" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_BatchEmptyEnumerable_ProducesNoOutputAndSuccess()
    {
        var result = await InvokeAsync("run", "absolute", "--batch", "{}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_BatchSingleElementEnumerable_ProducesSingleOutput()
    {
        var result = await InvokeAsync("run", "absolute", "--batch", "{5}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("5"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_ScalarInput_IsAcceptedAsSingleElementSequence()
    {
        var result = await InvokeAsync("run", "add(1)", "--input", "42");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("43"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_ScalarInput_WithWhitespaceWordSequence_IsAcceptedWithoutManualQuoting()
    {
        var result = await InvokeAsync("run", "upper", "--input", "nikola tesla");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("NIKOLA TESLA"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_ScalarInput_WithBacktickAndWhitespace_IsAcceptedWithoutManualQuoting()
    {
        var result = await InvokeAsync("run", "count-chars", "--input", "nik`ola tesla");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("13"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_ScalarNullToken_RemainsTypedNull()
    {
        var result = await InvokeAsync("run", "null-to-empty | count-chars", "--input", "null");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("0"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_MultipleInputOptions_EachOccurrenceDefinesOneRow()
    {
        var result = await InvokeAsync("run", "null-to-empty | count-chars", "--input", "1", "--input", "{2, 3}", "--input", "4");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "1", "0", "1" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_InputRecordWithDuplicateFields_ReturnsClearError()
    {
        var result = await InvokeAsync("run", "count", "--input", "{name := alice, name := bob}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Duplicate field 'name' in record literal."));
        });
    }

    [Test]
    public async Task Run_BatchOption_ScalarValue_ReturnsClearEnumerableError()
    {
        var result = await InvokeAsync("run", "add(1)", "--batch", "42");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The --batch option requires an enumerable value."));
        });
    }

    [Test]
    public async Task Run_BatchOption_RecordValue_ReturnsClearEnumerableError()
    {
        var result = await InvokeAsync("run", "count", "--batch", "{name := alice, age := 32}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The --batch option requires an enumerable value."));
        });
    }

    [Test]
    public async Task Run_SourceEnumerableExpression_EvaluatesEachRow()
    {
        var sourcePath = CreateTempFile("{1, -2, 3}");

        var result = await InvokeAsync("run", "absolute", "--source", sourcePath);

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsvScalar_EvaluatesEachValue()
    {
        var sourcePath = CreateTempFile($"name{Environment.NewLine}Alice{Environment.NewLine}Bob", ".csv");
        var result = await InvokeAsync("run", "upper", "--source", sourcePath, "--scalar");
        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsvScalarWithMultipleColumns_ReturnsClearError()
    {
        var sourcePath = CreateTempFile($"name,age{Environment.NewLine}Alice,32", ".csv");
        var result = await InvokeAsync("run", "upper", "--source", sourcePath, "--scalar");
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdErr, Does.Contain("exactly one column; found 2"));
        });
    }

    [Test]
    public async Task Run_SourceCsvScalarHeaderOnly_IsValid()
    {
        var sourcePath = CreateTempFile("name", ".csv");
        var result = await InvokeAsync("run", "upper", "--source", sourcePath, "--scalar");
        Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
    }

    [Test]
    public async Task Run_ScalarWithoutSource_ReturnsClearError()
    {
        var result = await InvokeAsync("run", "upper", "--input", "Alice", "--scalar");
        Assert.That(result.StdErr.Trim(), Is.EqualTo("The --scalar option requires --source."));
    }

    [Test]
    public async Task Run_SourceCsv_EvaluatesEachRecord()
    {
        var sourcePath = CreateTempFile($"name,age,country{Environment.NewLine}Alice,32,Belgium{Environment.NewLine}Bob,41,France{Environment.NewLine}Charlie,27,Germany", ".csv");

        var result = await InvokeAsync("run", "[name] | upper", "--source", sourcePath);

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB", "CHARLIE" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsvHeaderOnly_ProducesNoOutputAndSuccess()
    {
        var sourcePath = CreateTempFile("name,age,country", ".csv");

        var result = await InvokeAsync("run", "[name] | upper", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsvEmpty_ReturnsClearError()
    {
        var sourcePath = CreateTempFile(string.Empty, ".csv");

        var result = await InvokeAsync("run", "[name] | upper", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("is empty. A header row is required."));
        });
    }

    [Test]
    public async Task Run_SourceCsvDuplicateHeaders_ReturnsClearError()
    {
        var sourcePath = CreateTempFile($"name,name,country{Environment.NewLine}Alice,32,Belgium", ".csv");

        var result = await InvokeAsync("run", "[name] | upper", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("contains duplicate column name 'name'"));
        });
    }

    [Test]
    public async Task Run_SourceCsvInconsistentFields_ReturnsClearError()
    {
        var sourcePath = CreateTempFile($"name,age,country{Environment.NewLine}Alice,32,Belgium{Environment.NewLine}Bob,41", ".csv");

        var result = await InvokeAsync("run", "[name] | upper", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("ALICE"));
            Assert.That(result.StdErr, Does.Contain("contains 2 fields, but 3 fields were expected"));
        });
    }

    [Test]
    public async Task Run_SourceScalar_ReturnsClearError()
    {
        var sourcePath = CreateTempFile("42");

        var result = await InvokeAsync("run", "add(1)", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("returned a scalar value"));
            Assert.That(result.StdErr, Does.Contain("Expected an IEnumerable or IDataReader"));
        });
    }

    [Test]
    public async Task Run_SourceNull_ReturnsClearError()
    {
        RunCommand.ResolveSourceValue = static _ => null;

        var result = await InvokeAsync("run", "add(1)", "--source", "source.expr");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("returned null"));
            Assert.That(result.StdErr, Does.Contain("Expected an IEnumerable or IDataReader"));
        });
    }

    [Test]
    public async Task Run_SourceDataReader_EvaluatesEachRecord()
    {
        RunCommand.ResolveSourceValue = static _ =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Rows.Add("Alice");
            dataTable.Rows.Add("Bob");
            return dataTable.CreateDataReader();
        };

        var result = await InvokeAsync("run", "#0 | upper", "--source", "customers.sql");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsvPathWithGenericDataReader_DoesNotSkipFirstRow()
    {
        RunCommand.ResolveSourceValue = static _ =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Rows.Add("Alice");
            dataTable.Rows.Add("Bob");
            return dataTable.CreateDataReader();
        };

        var result = await InvokeAsync("run", "[name] | upper", "--source", "customers.csv");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceAndInputOptionsTogether_ReturnsClearError()
    {
        var sourcePath = CreateTempFile("{1,2,3}");

        var result = await InvokeAsync("run", "add(1)", "--source", sourcePath, "--input", "1");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The --source option cannot be combined with --input or --batch."));
        });
    }

    [Test]
    public async Task Run_MissingInputOption_ReturnsClearError()
    {
        var result = await InvokeAsync("run", "absolute");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The run command requires inputs. Provide --input, --batch, or --source."));
        });
    }

    [Test]
    public async Task Run_ExpressionFile_UsesSameExpressionLoadingRules()
    {
        var path = CreateTempFile("absolute");

        var result = await InvokeAsync("run", "--file", path, "--batch", "{1, -2, 3}");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_EvaluationFailure_StopsEnumerationAndReturnsFailure()
    {
        var result = await InvokeAsync("run", "add(1)", "--batch", "{1, unknown, 3}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("2"));
            Assert.That(result.StdErr, Does.Contain("Expression evaluation failed for input unknown"));
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
    public async Task Validate_ExpressionFile_ValidExpression_ReturnsSuccessAndMessage()
    {
        var path = CreateTempFile("absolute | add(5)");

        var result = await InvokeAsync("validate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("Expression is valid."));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Validate_ExpressionFile_ValidExpressionWithShortAlias_ReturnsSuccessAndMessage()
    {
        var path = CreateTempFile("absolute | add(5)");

        var result = await InvokeAsync("validate", "-f", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("Expression is valid."));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Validate_OpenExpressionRequiringInput_RemainsValid()
    {
        var result = await InvokeAsync("validate", "upper");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("Expression is valid."));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Validate_OpenOption_OpenExpressionRequiringInput_RemainsValid()
    {
        var result = await InvokeAsync("validate", "upper", "--open");

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
    public async Task Validate_ExpressionFile_InvalidExpression_ReturnsSourceAwareError()
    {
        var path = CreateTempFile("absolute | unknown(5)");

        var result = await InvokeAsync("validate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain($"The expression loaded from '{path}' is invalid:"));
            Assert.That(result.StdErr, Does.Contain("Unknown function 'unknown'."));
        });
    }

    [Test]
    public async Task Validate_DefaultOpen_IgnoresClosedValidationHook()
    {
        ValidateCommand.BuildClosedExpression = static (_, _) => throw new NotImplementedFunctionException("close-only-unknown");

        var result = await InvokeAsync("validate", "absolute | add(5)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("Expression is valid."));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Validate_ClosedValidationError_WithClosedOption_ReturnsValidationExitCode()
    {
        ValidateCommand.BuildClosedExpression = static (_, _) => throw new NotImplementedFunctionException("close-only-unknown");

        var result = await InvokeAsync("validate", "absolute | add(5)", "--closed");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unknown function 'close-only-unknown'."));
        });
    }

    [Test]
    public async Task Validate_ClosedValidationUnexpectedException_WithClosedOption_ReturnsUnexpectedInternalErrorExitCode()
    {
        ValidateCommand.BuildClosedExpression = static (_, _) => throw new InvalidOperationException("boom validate closed");

        var result = await InvokeAsync("validate", "absolute | add(5)", "--closed");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.UnexpectedInternalError));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unexpected error: boom validate closed"));
        });
    }

    [Test]
    public async Task Validate_ClosedOption_OpenExpression_ReturnsInputRequiredError()
    {
        var result = await InvokeAsync("validate", "upper", "--closed");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression cannot be evaluated without an input because it references 'upper'."));
        });
    }

    [Test]
    public async Task Validate_OpenAndClosedFlagsTogether_ReturnsClearError()
    {
        var result = await InvokeAsync("validate", "absolute | add(5)", "--open", "--closed");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Options --open and --closed cannot be used together."));
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
            Assert.That(result.StdOut, Does.Contain("run"));
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
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression must be supplied through exactly one source: inline or --file."));
        });
    }

    [Test]
    public async Task Validate_BothInlineAndExpressionFile_ReturnsClearError()
    {
        var path = CreateTempFile("absolute | add(5)");

        var result = await InvokeAsync("validate", "absolute | add(5)", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The expression cannot be provided both inline and through --file."));
        });
    }

    [Test]
    public async Task Validate_ExpressionFile_NotFound_ReturnsClearError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-validate-missing-{Guid.NewGuid():N}.expr");

        var result = await InvokeAsync("validate", "--file", path);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo($"Expression file '{path}' was not found."));
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

    private string CreateTempFile(string content, string extension = ".expr")
    {
        var path = Path.Combine(Path.GetTempPath(), $"expressif-{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        tempFilesToDelete.Add(path);
        return path;
    }

    private sealed record InvocationResult(int ExitCode, string StdOut, string StdErr);
}
