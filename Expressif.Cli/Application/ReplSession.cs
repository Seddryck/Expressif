using Expressif.Cli.Commands;
using Expressif.Cli.Expressions;

namespace Expressif.Cli.Application;

internal enum ReplErrorKind
{
    Input,
    Validation,
    Evaluation,
    Unexpected,
}

internal abstract record ReplResult;

internal sealed record ReplEvaluationResult(object? Value) : ReplResult;

internal sealed record ReplErrorResult(ReplErrorKind Kind, string Message) : ReplResult;

internal sealed class ReplSession
{
    private readonly IExpressionService expressions;
    private readonly Context bindingContext;
    private readonly EvaluationContext evaluationContext;
    private object? currentInput;

    public ReplSession(IExpressionService expressions)
        : this(expressions, new Context(), new EvaluationContext()) { }

    internal ReplSession(
        IExpressionService expressions,
        Context bindingContext,
        EvaluationContext evaluationContext)
        => (this.expressions, this.bindingContext, this.evaluationContext) =
            (expressions, bindingContext, evaluationContext);

    internal bool HasCurrentInput { get; private set; }

    internal object? CurrentInput => currentInput;

    public ReplResult Execute(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var trimmed = source.TrimStart();
        var isOpen = trimmed.StartsWith('|');
        if (isOpen && !HasCurrentInput)
            return new ReplErrorResult(ReplErrorKind.Input, "There is no current input. Evaluate a standalone expression first.");

        var code = isOpen
            ? trimmed.StartsWith("|>", StringComparison.Ordinal)
                ? trimmed
                : trimmed[1..].TrimStart()
            : source;
        IExpression expression;
        try
        {
            expression = (isOpen
                    ? expressions.CompileOpen(code, bindingContext)
                    : expressions.CompileClosed(code, bindingContext))
                .WithContext(evaluationContext);
        }
        catch (Exception exception) when (ExpressionFailureClassifier.IsValidation(exception))
        {
            return new ReplErrorResult(
                ReplErrorKind.Validation,
                CommandErrorFormatter.FormatValidationError(exception, code));
        }
        catch (ExpressionRequiresInputException exception)
        {
            return new ReplErrorResult(ReplErrorKind.Input, exception.Message);
        }
        catch (Exception exception)
        {
            return new ReplErrorResult(ReplErrorKind.Unexpected, $"Unexpected error: {exception.Message}");
        }

        try
        {
            var value = expressions.Evaluate(expression, isOpen ? currentInput : null);
            currentInput = value;
            HasCurrentInput = true;
            return new ReplEvaluationResult(value);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return new ReplErrorResult(
                ReplErrorKind.Evaluation,
                CommandErrorFormatter.FormatEvaluationError(exception));
        }
    }
}
