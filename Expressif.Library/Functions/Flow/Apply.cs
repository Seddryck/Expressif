namespace Expressif.Functions.Flow;

/// <summary>Evaluates an expression with the input value as its current context.</summary>
[Function(prefix: "", aliases: ["apply"])]
[Scope("flow")]
public sealed class Apply : IFunction
{
    private Func<IFunction> Expression { get; }

    /// <param name="expression">Specifies the expression evaluated against the input value.</param>
    public Apply(Func<IFunction> expression) => Expression = expression;

    public object? Evaluate(object? value) => Expression.Invoke().Evaluate(value);
}
