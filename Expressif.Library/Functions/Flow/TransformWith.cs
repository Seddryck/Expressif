using Expressif.Values;

namespace Expressif.Functions.Flow;

/// <summary>Transforms the results of one or more expressions with the same open expression and returns them as a tuple.</summary>
[Function(prefix: "")]
[Scope("flow")]
public sealed class TransformWith : IFunction<object?, TupleValue>
{
    private Func<IFunction> Operation { get; }
    private IReadOnlyList<Func<object?, object?>> Expressions { get; }

    /// <param name="operation">Open expression evaluated once against each result.</param>
    /// <param name="expressions">One or more expressions evaluated independently against the original input.</param>
    public TransformWith(Func<IFunction> operation, IEnumerable<Func<object?, object?>> expressions)
    {
        Operation = operation;
        Expressions = expressions.ToArray();
        if (Expressions.Count == 0)
            throw new MissingOrUnexpectedParametersFunctionException(nameof(TransformWith), 1);
    }

    public TupleValue Evaluate(object? value)
    {
        var operation = Operation.Invoke();
        return new TupleValue(Expressions.Select(expression => operation.Evaluate(expression.Invoke(value))).ToArray());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
