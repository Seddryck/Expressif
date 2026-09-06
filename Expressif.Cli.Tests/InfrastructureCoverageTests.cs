using Expressif.Cli.Expressions;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;

namespace Expressif.Cli.Tests;

public class InfrastructureCoverageTests
{
    [Test]
    public void OpenExpressionSource_WithOptions_RejectsUnsupportedOptions()
        => Assert.That(
            () => CreateInfrastructure().OpenExpressionSource("source.expr", ["mode=fast"]),
            Throws.TypeOf<FormatException>().With.Message.Contains("not supported"));

    [Test]
    public void OpenExpressionSource_CompilationFailure_IsSourceAware()
    {
        var expressions = new FakeExpressionService
        {
            CompileClosedHandler = static (_, _) => throw new NotImplementedFunctionException("missing"),
        };

        Assert.That(
            () => CreateInfrastructure(expressions).OpenExpressionSource("source.expr", []),
            Throws.TypeOf<FormatException>().With.Message.Contains("source.expr").And.Message.Contains("Unknown function 'missing'"));
    }

    [Test]
    public void OpenExpressionSource_EvaluationFailure_IsSourceAware()
    {
        var expressions = new FakeExpressionService
        {
            EvaluateHandler = static (_, _) => throw new InvalidOperationException("boom"),
        };

        Assert.That(
            () => CreateInfrastructure(expressions).OpenExpressionSource("source.expr", []),
            Throws.TypeOf<FormatException>().With.Message.Contains("could not be evaluated: boom"));
    }

    [TestCase((int)TextFileFailureKind.InvalidUtf8, "decoded as UTF-8")]
    [TestCase((int)TextFileFailureKind.Access, "could not be accessed")]
    public void OpenExpressionSource_TextReadFailure_IsNormalized(int kind, string expected)
    {
        var textFiles = new FakeTextReader
        {
            ReadHandler = path => throw new TextFileReadException(
                path,
                (TextFileFailureKind)kind,
                new IOException("failure")),
        };

        Assert.That(
            () => CreateInfrastructure(textFiles: textFiles).OpenExpressionSource("source.expr", []),
            Throws.TypeOf<FormatException>().With.Message.Contains(expected));
    }

    [Test]
    public void SourcePipeline_WithoutMatchingProvider_ReportsPath()
    {
        var infrastructure = CreateInfrastructure();
        var pipeline = new SourcePipeline([], infrastructure);

        Assert.That(
            () => pipeline.Read("virtual.source", []).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("input format").And.Message.Contains("--format"));
    }

    [Test]
    public void SourcePipeline_ProviderFailure_IsNormalized()
    {
        var infrastructure = CreateInfrastructure();
        var pipeline = new SourcePipeline([new ThrowingSourceProvider()], infrastructure);

        Assert.That(
            () => pipeline.Read("virtual.source", []).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("could not be resolved: boom"));
    }

    [Test]
    public void SourcePathValidator_RejectsBlankDirectoryAndMissingPaths()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => SourcePathValidator.Validate(" "), Throws.TypeOf<FormatException>());
            Assert.That(() => SourcePathValidator.Validate(Path.GetTempPath()), Throws.TypeOf<FormatException>());
            Assert.That(
                () => SourcePathValidator.Validate(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}")),
                Throws.TypeOf<FormatException>());
        });
    }

    [Test]
    public void InputSources_ParserFailures_IdentifyTheirOption()
    {
        var parser = new ThrowingInputValueParser();

        Assert.Multiple(() =>
        {
            Assert.That(
                () => new RepeatedInputSource(["bad"], parser).Read().ToArray(),
                Throws.TypeOf<FormatException>().With.Message.Contains("--input 'bad'"));
            Assert.That(
                () => new BatchInputSource("bad", parser).Read().ToArray(),
                Throws.TypeOf<FormatException>().With.Message.Contains("--batch 'bad'"));
        });
    }

    [Test]
    public void ExpressionService_CompilesAndEvaluatesOpenAndClosedExpressions()
    {
        var service = new ExpressionService();
        var context = new Context();

        Assert.Multiple(() =>
        {
            Assert.That(service.Evaluate(service.CompileOpen("add(1)", context), 2), Is.EqualTo(3));
            Assert.That(service.Evaluate(service.CompileClosed("1 | add(2)", context), null), Is.EqualTo(3));
        });
    }

    private static SourceInfrastructure CreateInfrastructure(
        IExpressionService? expressions = null,
        IStrictUtf8TextReader? textFiles = null)
        => new(
            expressions ?? new FakeExpressionService(),
            new CliInputValueParser(),
            textFiles ?? new FakeTextReader());

    private sealed class FakeExpressionService : IExpressionService
    {
        public Func<string, Context, IExpression> CompileClosedHandler { get; init; }
            = static (code, context) => Expression.CreateClosed(code, context);

        public Func<IExpression, object?, object?> EvaluateHandler { get; init; }
            = static (expression, input) => expression.Evaluate(input);

        public IExpression CompileOpen(string code, Context context) => Expression.Create(code, context);
        public IExpression CompileClosed(string code, Context context) => CompileClosedHandler(code, context);
        public object? Evaluate(IExpression expression, object? input) => EvaluateHandler(expression, input);
    }

    private sealed class FakeTextReader : IStrictUtf8TextReader
    {
        public Func<string, string> ReadHandler { get; init; } = static _ => "{1, 2}";

        public string Read(string path, bool requireContent = true) => ReadHandler(path);
    }

    private sealed class ThrowingSourceProvider : IFileSourceProvider
    {
        public SourceFormat? Format => null;
        public bool CanOpen(string path) => true;
        public object? Open(string path, IReadOnlyList<string> options) => throw new InvalidOperationException("boom");
    }

    private sealed class ThrowingInputValueParser : IInputValueParser
    {
        public object? Parse(string text) => throw new FormatException("invalid syntax");
        public object? ParseStrict(string text) => throw new FormatException("invalid syntax");
    }
}
