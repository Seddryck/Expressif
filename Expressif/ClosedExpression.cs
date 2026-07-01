using Expressif.Functions;

namespace Expressif;

public class ClosedExpression
{
    private readonly IFunction expression;

    public ClosedExpression(string code)
        : this(code, new Context()) { }

    public ClosedExpression(string code, IContext context)
        : this(code, context, new ExpressionFactory()) { }

    public ClosedExpression(string code, IContext context, ExpressionFactory factory)
        => expression = factory.Instantiate(code, context);

    public object? Evaluate()
        => expression.Evaluate(null);
}
