using Expressif.Observability;

namespace Expressif;

internal sealed class ObservedExpression : IExpression
{
    private readonly IExpression expression;
    private readonly IExpressionObserver observer;

    public ObservedExpression(IExpression expression, IExpressionObserver observer)
        => (this.expression, this.observer) = (expression, observer);

    public object? Evaluate(object? value)
    {
        using var observation = observer.Begin(ExpressionObservationStage.Evaluate);
        var frame = new EvaluationFrame(value, value, observation);
        return expression.Evaluate(frame.Current);
    }

    public IExpression WithContext(EvaluationContext context)
        => new ObservedExpression(expression.WithContext(context), observer);
}
