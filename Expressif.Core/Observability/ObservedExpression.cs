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
        try
        {
            var result = expression.Evaluate(frame.Current);
            observation.Complete();
            return result;
        }
        catch (Exception exception)
        {
            observation.Fail(exception);
            throw;
        }
    }

    public IExpression WithContext(EvaluationContext context)
        => new ObservedExpression(expression.WithContext(context), observer);
}
