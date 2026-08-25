using Expressif.Bindings;
using Expressif.Functions;

namespace Expressif;

public class Expression : IExpression
{
    private readonly IFunction expression;
    private readonly EvaluationContext context;

    public static IExpression Create(string text)
        => new ExpressionFactory().Create(text);

    public static IExpression Create(string text, IContext context)
        => new ExpressionFactory(binder: new ExpressionBinder(context)).Create(text);

    public static IExpression CreateClosed(string text)
        => new ExpressionFactory().CreateClosed(text);

    public static IExpression CreateClosed(string text, IContext context)
        => new ExpressionFactory(binder: new ExpressionBinder(context)).CreateClosed(text);

    internal Expression(IFunction expression)
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
