using Expressif.Bindings;
using Expressif.Functions;

namespace Expressif;

public class Expression : IExpression
{
    private readonly IFunction expression;

    public static IExpression Create(string text)
        => new ExpressionFactory().Create(text);

    public static IExpression Create(string text, IContext context)
        => new ExpressionFactory(binder: new ExpressionBinder(context)).Create(text);

    internal Expression(IFunction expression)
        => this.expression = expression;

    public object? Evaluate(object? value) => expression.Evaluate(value);
}
