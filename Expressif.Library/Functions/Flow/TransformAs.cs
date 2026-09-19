using Expressif.Values;

namespace Expressif.Functions.Flow;

/// <summary>Transforms one or more named expression results with the same open expression and returns them as a record.</summary>
[Function(prefix: "")]
[Scope("flow")]
public sealed class TransformAs : IFunction<object?, RecordValue>
{
    private Func<IFunction> Operation { get; }
    private IReadOnlyList<NamedExpressionEvaluator> Expressions { get; }

    /// <param name="operation">Open expression evaluated once against each named result.</param>
    /// <param name="expressions">One or more named expressions evaluated independently against the original input.</param>
    public TransformAs(Func<IFunction> operation, IEnumerable<NamedExpressionEvaluator> expressions)
    {
        Operation = operation;
        Expressions = expressions.ToArray();
        if (Expressions.Count == 0)
            throw new MissingOrUnexpectedParametersFunctionException(nameof(TransformAs), 1);
    }

    public RecordValue Evaluate(object? value)
    {
        var operation = Operation.Invoke();
        var record = new RecordValue();
        foreach (var expression in Expressions)
            record.Set(expression.Name, operation.Evaluate(expression.Evaluator.Invoke(value)));

        return record;
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

public sealed record NamedExpressionEvaluator(string Name, Func<object?, object?> Evaluator);
