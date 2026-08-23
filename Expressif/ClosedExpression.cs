using Expressif.Functions;

namespace Expressif;

public class ClosedExpression
{
    private readonly IFunction expression;

    public ClosedExpression(string code)
        : this(code, new Context()) { }

    public ClosedExpression(string code, IContext context)
        : this(code, context, new Functions.FunctionFactory()) { }

    public ClosedExpression(string code, IContext context, Functions.FunctionFactory factory)
        => expression = factory.InstantiateClosed(code, context);

    public object? Evaluate()
        => expression.Evaluate(null);
}
