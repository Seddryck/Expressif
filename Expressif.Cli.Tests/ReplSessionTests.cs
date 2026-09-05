using Expressif.Cli.Application;
using Expressif.Cli.Expressions;

namespace Expressif.Cli.Tests;

public class ReplSessionTests
{
    [Test]
    public void Execute_ClosedThenOpenExpression_UsesSuccessfulResultAsCurrentInput()
    {
        var session = new ReplSession(new ExpressionService());

        var first = session.Execute("\"  Alice  \" | trim");
        var second = session.Execute("| upper");

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(new ReplEvaluationResult("Alice")));
            Assert.That(second, Is.EqualTo(new ReplEvaluationResult("ALICE")));
            Assert.That(session.HasCurrentInput, Is.True);
            Assert.That(session.CurrentInput, Is.EqualTo("ALICE"));
        });
    }

    [Test]
    public void Execute_LeadingMapPipeline_PreservesPipelineOperator()
    {
        var session = new ReplSession(new ExpressionService());
        _ = session.Execute("{1, 2, 3}");

        var result = session.Execute("|> add(1)");

        Assert.That(result, Is.TypeOf<ReplEvaluationResult>());
        Assert.That(((ReplEvaluationResult)result).Value, Is.EqualTo(new object?[] { 2, 3, 4 }));
    }

    [Test]
    public void Execute_LeadingPipelineWithoutCurrentInput_ReturnsInputError()
    {
        var session = new ReplSession(new ExpressionService());

        var result = session.Execute("| upper");

        Assert.That(result, Is.EqualTo(new ReplErrorResult(
            ReplErrorKind.Input,
            "There is no current input. Evaluate a standalone expression first.")));
        Assert.That(session.HasCurrentInput, Is.False);
    }

    [Test]
    public void Execute_NullResult_IsRetainedAsCurrentInput()
    {
        var session = new ReplSession(new ExpressionService());

        var result = session.Execute("{} | first");

        Assert.That(result, Is.EqualTo(new ReplEvaluationResult(null)));
        Assert.That(session.HasCurrentInput, Is.True);
        Assert.That(session.CurrentInput, Is.Null);
        Assert.That(session.Execute("| null-to-empty"), Is.TypeOf<ReplEvaluationResult>());
    }

    [Test]
    public void Execute_FailedExpression_DoesNotReplaceCurrentInput()
    {
        var session = new ReplSession(new ExpressionService());
        _ = session.Execute("41 | add(1)");

        var result = session.Execute("| unknown-function");

        Assert.That(result, Is.TypeOf<ReplErrorResult>());
        Assert.That(session.CurrentInput, Is.EqualTo(42));
    }

    [Test]
    public void Execute_RuntimeFailure_ReturnsEvaluationErrorAndPreservesInput()
    {
        var expressions = new ThrowingEvaluationService();
        var session = new ReplSession(expressions);
        expressions.Result = 17;
        _ = session.Execute("first");
        expressions.Exception = new InvalidOperationException("boom");

        var result = session.Execute("| second");

        Assert.That(result, Is.EqualTo(new ReplErrorResult(
            ReplErrorKind.Evaluation,
            $"Evaluation error [EXPR3001]:{Environment.NewLine}boom")));
        Assert.That(session.CurrentInput, Is.EqualTo(17));
    }

    [Test]
    public void Execute_ReusesOneEvaluationContext()
    {
        var expressions = new TrackingExpressionService();
        var context = new EvaluationContext();
        var session = new ReplSession(expressions, new Context(), context);

        _ = session.Execute("1");
        _ = session.Execute("2");

        Assert.That(expressions.Contexts, Is.EqualTo(new[] { context, context }));
    }

    [Test]
    public void Run_EndOfInput_ExitsSuccessfullyAndFormatsResults()
    {
        var terminal = new FakeTerminal("1 | add(1)", "| multiply(3)");
        var host = new ReplHost(new ReplSession(new ExpressionService()), terminal);

        var exitCode = host.Run();

        Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
        Assert.That(terminal.Results, Is.EqualTo(new[] { "2", "6" }));
        Assert.That(terminal.Errors, Is.Empty);
    }

    [Test]
    public void Run_Cancellation_ExitsSuccessfully()
    {
        var host = new ReplHost(
            new ReplSession(new ExpressionService()),
            new FakeTerminal(new OperationCanceledException()));

        Assert.That(host.Run(), Is.EqualTo(ExitCodes.Success));
    }

    private sealed class FakeTerminal : IReplTerminal
    {
        private readonly Queue<string?> lines;
        private readonly Exception? exception;

        public FakeTerminal(params string[] lines)
            => this.lines = new Queue<string?>(lines.Append(null));

        public FakeTerminal(Exception exception)
            => (lines, this.exception) = (new Queue<string?>(), exception);

        public List<string> Results { get; } = [];
        public List<string> Errors { get; } = [];

        public string? ReadLine(string prompt, CancellationToken cancellationToken)
        {
            if (exception is not null)
                throw exception;
            return lines.Dequeue();
        }

        public void WriteResult(string value) => Results.Add(value);
        public void WriteError(string message) => Errors.Add(message);
    }

    private sealed class TrackingExpressionService : IExpressionService
    {
        public List<EvaluationContext> Contexts { get; } = [];

        public IExpression CompileOpen(string code, Context context) => new TrackingExpression(this);
        public IExpression CompileClosed(string code, Context context) => new TrackingExpression(this);
        public object? Evaluate(IExpression expression, object? input) => expression.Evaluate(input);

        private sealed class TrackingExpression(TrackingExpressionService owner) : IExpression
        {
            public object? Evaluate(object? value) => value;

            public IExpression WithContext(EvaluationContext context)
            {
                owner.Contexts.Add(context);
                return this;
            }
        }
    }

    private sealed class ThrowingEvaluationService : IExpressionService
    {
        public object? Result { get; set; }
        public Exception? Exception { get; set; }

        public IExpression CompileOpen(string code, Context context) => new StubExpression();
        public IExpression CompileClosed(string code, Context context) => new StubExpression();

        public object? Evaluate(IExpression expression, object? input)
            => Exception is null ? Result : throw Exception;

        private sealed class StubExpression : IExpression
        {
            public object? Evaluate(object? value) => value;
            public IExpression WithContext(EvaluationContext context) => this;
        }
    }
}
