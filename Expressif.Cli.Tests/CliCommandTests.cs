using System.Data;
using Expressif.Cli.Application;
using Expressif.Cli.Commands;
using Expressif.Cli.Expressions;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;
using Expressif.Functions.Catalog;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class CliCommandTests
{
    private readonly List<string> tempFilesToDelete = [];
    private FakeExpressionService expressions = null!;
    private Func<string, object?>? sourceResolver;

    [SetUp]
    public void SetUp()
    {
        expressions = new FakeExpressionService();
        sourceResolver = null;
    }

    [Test]
    public void DiagnosticWriter_Colorize_UsesAnsiRedOnlyWhenEnabled()
    {
        const string message = "Syntax error [EXPR1001]";

        Assert.Multiple(() =>
        {
            Assert.That(
                CommandDiagnosticWriter.Colorize(message, useColor: true),
                Is.EqualTo("\u001b[31mSyntax error [EXPR1001]\u001b[0m"));
            Assert.That(
                CommandDiagnosticWriter.Colorize(message, useColor: false),
                Is.EqualTo(message));
        });
    }

    [TearDown]
    public void TearDown()
    {
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

    [TestCase("even", "4", "true")]
    [TestCase("even", "5", "false")]
    [TestCase("is-even", "4", "true")]
    public async Task Evaluate_PredicateOnlyExpression_ReturnsBoolean(string expression, string input, string expected)
    {
        var result = await InvokeAsync("evaluate", expression, "--input", input);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo(expected));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [TestCase("4", "false")]
    [TestCase("6", "true")]
    [TestCase("7", "false")]
    public async Task Evaluate_ComposedPredicateOnlyExpression_ReturnsBoolean(string input, string expected)
    {
        var result = await InvokeAsync("evaluate", "even |AND greater-than(5)", "--input", input);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo(expected));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [TestCase("-1", "true")]
    [TestCase("6", "true")]
    [TestCase("3", "false")]
    [TestCase("5", "false")]
    public async Task Evaluate_NestedLowercasePredicateExpression_ReturnsBoolean(string input, string expected)
    {
        var result = await InvokeAsync(
            "evaluate",
            "(even |and greater-than(5)) |or less-than(0)",
            "--input",
            input);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo(expected));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [TestCase("-3", "true")]
    [TestCase("-2", "false")]
    [TestCase("6", "true")]
    [TestCase("7", "false")]
    public async Task Evaluate_TwoNestedPredicateBranches_ReturnsBoolean(string input, string expected)
    {
        var result = await InvokeAsync(
            "evaluate",
            "(odd |and less-than(0)) |or (even |and greater-than(5))",
            "--input",
            input);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo(expected));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_PredicateClosedInputExpression_ResolvesNestedFunction()
    {
        var result = await InvokeAsync(
            "evaluate",
            "filter(greater-than(17 | add(17)))",
            "--input",
            "{10,12,13}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("{}"));
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
    public async Task Evaluate_ArrayLiteralInput_ReturnsAggregatedResult()
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
    public async Task Evaluate_RecordLiteralInput_ReturnsFieldValue()
    {
        var result = await InvokeAsync("evaluate", ".loc", "--input", "{loc:=\"mons\", temp:=17.5}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("mons"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_TupleLiteralInput_ReturnsTupleField()
    {
        var result = await InvokeAsync("evaluate", "tuple-second", "--input", "T(\"mons\", 17.5)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("17.5"));
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
        expressions.CompileClosedHandler = static (_, _) => throw new ExpressionRequiresInputException("upper");
        expressions.CompileOpenHandler = static (_, _) => throw new NotImplementedFunctionException("unknown");

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
        expressions.CompileClosedHandler = static (_, _) => throw new InvalidOperationException("boom closed compile");

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
        expressions.CompileOpenHandler = static (_, _) => throw new InvalidOperationException("boom open compile");

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
        expressions.EvaluateHandler = static (_, _) => throw new InvalidOperationException("boom closed runtime");

        var result = await InvokeAsync("evaluate", "5 | add(3)");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.StartWith("Evaluation error [EXPR3001]:"));
            Assert.That(result.StdErr, Does.Contain("boom closed runtime"));
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
    public async Task Run_ExplicitNullToken_RemainsTypedNull()
    {
        var result = await InvokeAsync("run", "null-to-empty | count-chars", "--input", "#null");

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
        var result = await InvokeAsync("run", "count", "--input", "{name := \"alice\", name := \"bob\"}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.StartWith("Runtime error [EXPR4001]:"));
            Assert.That(result.StdErr, Does.Contain("Duplicate field 'name' in record literal."));
            Assert.That(result.StdErr, Does.Contain("Input row: 1"));
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
        var result = await InvokeAsync("run", "count", "--batch", "{name := \"alice\", age := 32}");

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
    public async Task Evaluate_SourceCsv_EvaluatesCompleteRecordArrayOnce()
    {
        var sourcePath = CreateTempFile($"name,age{Environment.NewLine}Alice,32{Environment.NewLine}Bob,41", ".csv");
        var result = await InvokeAsync("evaluate", "count", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("2"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_SourceCsvScalar_EvaluatesCompleteValueArrayOnce()
    {
        var sourcePath = CreateTempFile($"value{Environment.NewLine}10{Environment.NewLine}20{Environment.NewLine}30", ".csv");
        var result = await InvokeAsync("evaluate", "sum", "--source", sourcePath, "--scalar");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("60"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_SourceCsv_WithSourceOptions_UsesConfiguredProfile()
    {
        var sourcePath = CreateTempFile($"value;ignored{Environment.NewLine}10;x{Environment.NewLine}20;y", ".csv");
        var result = await InvokeAsync(
            "evaluate", "count", "--source", sourcePath,
            "--source-option", "delimiter=\";\"");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("2"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [TestCase("name,age", false)]
    [TestCase("name", true)]
    public async Task Evaluate_SourceCsvHeaderOnly_EvaluatesEmptyArray(string header, bool scalar)
    {
        var sourcePath = CreateTempFile(header, ".csv");
        var arguments = scalar
            ? new[] { "evaluate", "count", "--source", sourcePath, "--scalar" }
            : new[] { "evaluate", "count", "--source", sourcePath };
        var result = await InvokeAsync(arguments);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("0"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Evaluate_SourceAndInputTogether_ReturnsClearError()
    {
        var sourcePath = CreateTempFile("name", ".csv");
        var result = await InvokeAsync("evaluate", "count", "--source", sourcePath, "--input", "value");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The --source option cannot be combined with --input."));
        });
    }

    [TestCase("--scalar", "The --scalar option requires --source.")]
    [TestCase("--source-option", "The --source-option option requires --source.")]
    public async Task Evaluate_SourceDependentOptionWithoutSource_ReturnsClearError(string option, string expectedError)
    {
        var arguments = option == "--source-option"
            ? new[] { "evaluate", "count", option, "header=#true" }
            : new[] { "evaluate", "count", option };
        var result = await InvokeAsync(arguments);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdErr.Trim(), Is.EqualTo(expectedError));
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
    public async Task Run_HeaderlessSourceCsvScalar_EvaluatesFirstAndFollowingValues()
    {
        var sourcePath = CreateTempFile($"12{Environment.NewLine}5{Environment.NewLine}42{Environment.NewLine}17", ".csv");
        var result = await InvokeAsync(
            "run", "add(5)", "--source", sourcePath, "--scalar",
            "--source-option", "header=#false");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "17", "10", "47", "22" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_HeaderlessSourceCsv_UsesGeneratedColumnNames()
    {
        var sourcePath = CreateTempFile($"Alice,32{Environment.NewLine}Bob,41", ".csv");
        var result = await InvokeAsync(
            "run", ".column1 | upper", "--source", sourcePath,
            "--source-option", "header=#false");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_HeaderlessSourceCsvInconsistentFields_ReturnsClearError()
    {
        var sourcePath = CreateTempFile($"Alice,32{Environment.NewLine}Bob", ".csv");
        var result = await InvokeAsync(
            "run", ".column1 | upper", "--source", sourcePath,
            "--source-option", "header=#false");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("ALICE"));
            Assert.That(result.StdErr, Does.Contain("CSV record 2").And.Contain("contains 1 fields, but 2 fields were expected"));
        });
    }

    [Test]
    public async Task Run_SourceCsv_EvaluatesEachRecord()
    {
        var sourcePath = CreateTempFile($"name,age,country{Environment.NewLine}Alice,32,Belgium{Environment.NewLine}Bob,41,France{Environment.NewLine}Charlie,27,Germany", ".csv");

        var result = await InvokeAsync("run", ".name | upper", "--source", sourcePath);

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB", "CHARLIE" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsv_PassesRecordValueToExpression()
    {
        var sourcePath = CreateTempFile($"name,age{Environment.NewLine}Alice,32", ".csv");

        var result = await InvokeAsync("run", "record(...)", "--source", sourcePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("{name := Alice, age := \"32\"}"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }
    public async Task Run_SourceCsv_WithRepeatedSourceOptions_UsesConfiguredProfile()
    {
        var sourcePath = CreateTempFile($"name;country{Environment.NewLine} Alice;Belgium{Environment.NewLine} Bob;France", ".csv");

        var result = await InvokeAsync(
            "run", ".name | upper", "--source", sourcePath,
            "--source-option", "delimiter=\";\"",
            "--source-option", "skip-initial-space=#true");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsv_WithExplicitHeader_UsesPocketCsvHeaderNames()
    {
        var sourcePath = CreateTempFile($"name,country{Environment.NewLine}Alice,Belgium{Environment.NewLine}Bob,France", ".csv");

        var result = await InvokeAsync(
            "run", ".name | upper", "--source", sourcePath,
            "--source-option", "header=#true");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceOption_WithoutSource_ReturnsClearError()
    {
        var result = await InvokeAsync("run", "absolute", "--input", "1", "--source-option", "header=#true");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("The --source-option option requires --source."));
        });
    }

    [Test]
    public async Task Run_SourceCsv_InvalidSourceOption_ReturnsClearError()
    {
        var sourcePath = CreateTempFile("name,age", ".csv");

        var result = await InvokeAsync("run", ".name", "--source", sourcePath, "--source-option", "delimiter=\"long\"");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("Invalid CSV source option 'delimiter' with value '\"long\"'"));
        });
    }

    [Test]
    public async Task Run_SourceCsv_UnknownSourceOption_ListsValidOptions()
    {
        var sourcePath = CreateTempFile("12", ".csv");

        var result = await InvokeAsync(
            "run", "add(5)", "--source", sourcePath, "--scalar",
            "--source-option", "headers=#false");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("Unknown CSV source option 'headers' with value '#false'."));
            Assert.That(result.StdErr, Does.Contain("Valid source options: delimiter, line-terminator"));
            Assert.That(result.StdErr, Does.Contain("array-prefix, array-suffix."));
        });
    }

    [Test]
    public async Task Run_SourceCsvHeaderOnly_ProducesNoOutputAndSuccess()
    {
        var sourcePath = CreateTempFile("name,age,country", ".csv");

        var result = await InvokeAsync("run", ".name | upper", "--source", sourcePath);

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

        var result = await InvokeAsync("run", ".name | upper", "--source", sourcePath);

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

        var result = await InvokeAsync("run", ".name | upper", "--source", sourcePath);

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

        var result = await InvokeAsync("run", ".name | upper", "--source", sourcePath);

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
        sourceResolver = static _ => null;

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
        sourceResolver = static _ =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Rows.Add("Alice");
            dataTable.Rows.Add("Bob");
            return dataTable.CreateDataReader();
        };

        var result = await InvokeAsync("run", ".name | upper", "--source", "customers.sql");

        var outputs = result.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(outputs, Is.EqualTo(new[] { "ALICE", "BOB" }));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceDataReader_PassesRecordValueToExpression()
    {
        sourceResolver = static _ =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Rows.Add("Alice");
            return dataTable.CreateDataReader();
        };

        var result = await InvokeAsync("run", "record(...)", "--source", "customers.sql");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("{name := Alice}"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Run_SourceCsvPathWithGenericDataReader_DoesNotSkipFirstRow()
    {
        sourceResolver = static _ =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Rows.Add("Alice");
            dataTable.Rows.Add("Bob");
            return dataTable.CreateDataReader();
        };

        var result = await InvokeAsync("run", ".name | upper", "--source", "customers.csv");

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
        var result = await InvokeAsync("run", "add(1)", "--batch", "{1, \"unknown\", 3}");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.StdOut.Trim(), Is.EqualTo("2"));
            Assert.That(result.StdErr, Does.Contain("Evaluation error [EXPR3001]:"));
            Assert.That(result.StdErr, Does.Contain("Input row: 2"));
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
    public async Task ValidateEvaluateAndRun_InvalidSyntax_RenderEquivalentDiagnostics()
    {
        const string expression = "lower | add(,)";

        var validateResult = await InvokeAsync("validate", expression);
        var evaluateResult = await InvokeAsync("evaluate", expression, "--input", "value");
        var runResult = await InvokeAsync("run", expression, "--input", "value");

        Assert.Multiple(() =>
        {
            Assert.That(validateResult.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(evaluateResult.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(runResult.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(validateResult.StdErr, Is.EqualTo(evaluateResult.StdErr));
            Assert.That(validateResult.StdErr, Is.EqualTo(runResult.StdErr));
            Assert.That(validateResult.StdErr, Does.Contain("Syntax error [EXPR1001] at line 1, column"));
            Assert.That(validateResult.StdErr, Does.Contain(expression));
            Assert.That(validateResult.StdErr, Does.Contain("^"));
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
        expressions.CompileClosedHandler = static (_, _) => throw new NotImplementedFunctionException("close-only-unknown");

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
        expressions.CompileClosedHandler = static (_, _) => throw new NotImplementedFunctionException("close-only-unknown");

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
        expressions.CompileClosedHandler = static (_, _) => throw new InvalidOperationException("boom validate closed");

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
            Assert.That(result.StdOut, Does.Contain("help"));
            Assert.That(result.StdOut, Does.Contain("version"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_CanonicalName_DisplaysFunctionDocumentation()
    {
        var result = await InvokeAsync("help", "reverse");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith($"array →{Environment.NewLine}reverse() → array"));
            Assert.That(result.StdOut, Does.Contain("Scope:   Array"));
            Assert.That(result.StdOut, Does.Contain("Aliases: reverse"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_Alias_DisplaysCanonicalFunctionAndAlias()
    {
        var result = await InvokeAsync("help", "array-to-broadcast");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith(
                $"array →{Environment.NewLine}broadcast({Environment.NewLine}    accumulator: accumulator{Environment.NewLine}) → array"));
            Assert.That(result.StdOut, Does.Contain("Aliases: array-to-broadcast"));
            Assert.That(result.StdOut, Does.Contain("Parameters:"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_OptionalParameter_MarksParameterAsOptional()
    {
        var result = await InvokeAsync("help", "after-substring");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith(
                $"text →{Environment.NewLine}after-substring({Environment.NewLine}    substring: text,{Environment.NewLine}    count?: integer{Environment.NewLine}) → text"));
            Assert.That(result.StdOut, Does.Contain("count?"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_DynamicFunction_DisplaysAnyContract()
    {
        var result = await InvokeAsync("help", "field");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith(
                $"any →{Environment.NewLine}field({Environment.NewLine}    name: text{Environment.NewLine}) → any"));
        });
    }

    [Test]
    public async Task Help_FunctionWithExamples_DisplaysExamplesBeforeAliasesAndScope()
    {
        var result = await InvokeAsync("help", "add");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith(
                $"numeric →{Environment.NewLine}add({Environment.NewLine}    value: numeric,{Environment.NewLine}    times?: integer{Environment.NewLine}) → numeric"));
            Assert.That(result.StdOut, Does.Contain("Returns the sum of the input value and the parameter value."));
            Assert.That(result.StdOut, Does.Contain("times  integer (optional)"));
            Assert.That(result.StdOut, Does.Contain("  10 | add(5)      → 15"));
            Assert.That(result.StdOut, Does.Contain("  10 | add(5, 2)   → 20"));
            Assert.That(result.StdOut.IndexOf("Examples:", StringComparison.Ordinal),
                Is.LessThan(result.StdOut.IndexOf("Aliases:", StringComparison.Ordinal)));
            Assert.That(result.StdOut.IndexOf("Aliases:", StringComparison.Ordinal),
                Is.LessThan(result.StdOut.IndexOf("Scope:", StringComparison.Ordinal)));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_UnknownName_DisplaysCloseMatchAndInvalidInputExitCode()
    {
        var result = await InvokeAsync("help", "revers");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr, Does.Contain("Unknown function 'revers'."));
            Assert.That(result.StdErr, Does.Contain("Did you mean: reverse?"));
        });
    }

    [Test]
    public async Task Help_List_DisplaysFunctionsFromSeveralScopes()
    {
        var result = await InvokeAsync("help", "--list");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.Contain("Array:"));
            Assert.That(result.StdOut, Does.Contain("Numeric:"));
            Assert.That(result.StdOut, Does.Contain("Record:"));
            Assert.That(result.StdOut, Does.Contain("Temporal:"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_ScopeCaseInsensitive_DisplaysOnlyRequestedScope()
    {
        var result = await InvokeAsync("help", "--scope", "record");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.StdOut, Does.StartWith("Record:"));
            Assert.That(result.StdOut, Does.Contain("  field"));
            Assert.That(result.StdOut, Does.Not.Contain("Array:"));
            Assert.That(result.StdErr, Is.Empty);
        });
    }

    [Test]
    public async Task Help_ConflictingModes_ReturnsInvalidInputExitCode()
    {
        var result = await InvokeAsync("help", "reverse", "--list");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Specify exactly one function name, --list, or --scope."));
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
        expressions.CompileOpenHandler = static (_, _) => throw new InvalidOperationException("boom");

        var result = await InvokeAsync("validate", "absolute");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(ExitCodes.UnexpectedInternalError));
            Assert.That(result.StdOut, Is.Empty);
            Assert.That(result.StdErr.Trim(), Is.EqualTo("Unexpected error: boom"));
        });
    }

    private async Task<InvocationResult> InvokeAsync(params string[] args)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;

        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        Console.SetOut(stdout);
        Console.SetError(stderr);

        try
        {
            var values = new CliInputValueParser();
            var textFiles = new StrictUtf8TextReader();
            var infrastructure = new SourceInfrastructure(expressions, values, textFiles);
            IFileSourceProvider[] providers = sourceResolver is null
                ? [new CsvFileSourceProvider(infrastructure), new ExpressionFileSourceProvider(infrastructure)]
                : [new FakeFileSourceProvider(sourceResolver)];
            var sources = new SourcePipeline(providers, infrastructure);
            var composition = new CliComposition(
                new ParseHandler(new SyntaxService()),
                new BindHandler(new SyntaxService()),
                new EvaluateHandler(expressions, values, sources),
                new RunHandler(expressions, values, textFiles, sources),
                new ValidateHandler(expressions),
                new HelpHandler(new FunctionCatalogService(FunctionCatalog.Default)),
                textFiles);
            var exitCode = await CliInvoker.InvokeAsync(args, composition);
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

    private sealed class FakeFileSourceProvider(Func<string, object?> resolve) : IFileSourceProvider
    {
        public bool CanOpen(string path) => true;

        public object? Open(string path, IReadOnlyList<string> options) => resolve(path);
    }

    private sealed class FakeExpressionService : IExpressionService
    {
        public Func<string, Context, IExpression> CompileOpenHandler { get; set; }
            = static (code, context) => Expression.Create(code, context);

        public Func<string, Context, IExpression> CompileClosedHandler { get; set; }
            = static (code, context) => Expression.CreateClosed(code, context);

        public Func<IExpression, object?, object?> EvaluateHandler { get; set; }
            = static (expression, input) => expression.Evaluate(input);

        public IExpression CompileOpen(string code, Context context) => CompileOpenHandler(code, context);
        public IExpression CompileClosed(string code, Context context) => CompileClosedHandler(code, context);
        public object? Evaluate(IExpression expression, object? input) => EvaluateHandler(expression, input);
    }
}
