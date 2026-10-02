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
        var observation = ExpressionObservationScope.Create(observer, ExpressionObservationStage.Evaluate);
        if (observation is null)
            return expression.Evaluate(value);
        var activation = observation.Activate();
        try
        {
            var result = expression.Evaluate(value);
            ExpressionObservationScope.Complete(observation);
            return result;
        }
        catch (Exception exception)
        {
            ExpressionObservationScope.Fail(observation, exception);
            throw;
        }
        finally
        {
            activation.Dispose();
            ExpressionObservationScope.Dispose(observation);
        }
    }

    public IExpression WithContext(EvaluationContext context)
        => new ObservedExpression(expression.WithContext(context), observer);
}
