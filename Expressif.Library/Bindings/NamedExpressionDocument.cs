using Expressif.Functions;

namespace Expressif.Bindings;

/// <summary>
/// An executable logical document containing reusable named expressions and an optional entry expression.
/// </summary>
public sealed class NamedExpressionDocument : IExpression
{
    private readonly IFunction? entry;
    private readonly IReadOnlyDictionary<string, NamedExpressionInvoker> definitions;
    private readonly EvaluationContext context;

    internal NamedExpressionDocument(
        IFunction? entry,
        IReadOnlyDictionary<string, NamedExpressionInvoker> definitions,
        EvaluationContext? context = null)
        => (this.entry, this.definitions, this.context) =
            (entry, definitions, context ?? EvaluationContext.Empty);

    public IReadOnlyCollection<string> Definitions => definitions.Keys.ToArray();
    public bool HasEntry => entry is not null;

    public object? Evaluate(object? value)
    {
        using var evaluation = EvaluationRuntime.Enter(new EvaluationFrame(value, value), context);
        using var scope = NamedExpressionRuntime.Enter(definitions);
        return entry is null ? value : EvaluationRuntime.CaptureDeferredResult(entry.Evaluate(value));
    }

    public object? Invoke(string name, object? input, params object?[] arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(arguments);
        using var evaluation = EvaluationRuntime.Enter(new EvaluationFrame(input, input), context);
        using var scope = NamedExpressionRuntime.Enter(definitions);
        return EvaluationRuntime.CaptureDeferredResult(NamedExpressionRuntime.Invoke(name, input, arguments));
    }

    public IExpression WithContext(EvaluationContext context)
        => new NamedExpressionDocument(entry, definitions, context ?? throw new ArgumentNullException(nameof(context)));
}
