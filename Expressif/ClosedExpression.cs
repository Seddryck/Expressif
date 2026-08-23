using Expressif.Functions;
using Expressif.Bindings;
using Expressif.Syntax;

namespace Expressif;

public class ClosedExpression
{
    private readonly IFunction expression;

    public static ClosedExpression Create(string text)
        => Create(text, new Context());

    public static ClosedExpression Create(string text, IContext context)
    {
        var syntax = ExpressionParser.Parse(text);
        var boundExpression = new ExpressifBinder().Bind(syntax);
        var expression = new Functions.FunctionFactory().InstantiateClosed(boundExpression, context);
        return new ClosedExpression(expression);
    }

    private ClosedExpression(IFunction expression)
        => this.expression = expression;

    public object? Evaluate()
        => expression.Evaluate(null);
}
