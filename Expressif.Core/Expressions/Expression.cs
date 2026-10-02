using Expressif.Bindings;
using Expressif.Functions;

namespace Expressif;

public class Expression : IExpression
{
    private readonly IFunction expression;
    private readonly EvaluationContext context;

    public static IExpression Create(string text, IExpressionBinder binder)
        => new ExpressionFactory(binder).Create(text);

    public static IExpression CreateClosed(string text, IExpressionBinder binder)
        => new ExpressionFactory(binder).CreateClosed(text);

    public Expression(IFunction expression)
        : this(expression, EvaluationContext.Empty) { }

    private Expression(IFunction expression, EvaluationContext context)
        => (this.expression, this.context) = (expression, context);

    public object? Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Enter(new EvaluationFrame(value, value), context);
        return expression.Evaluate(value);
    }

    public IExpression WithContext(EvaluationContext context)
        => new Expression(expression, context ?? throw new ArgumentNullException(nameof(context)));
}
