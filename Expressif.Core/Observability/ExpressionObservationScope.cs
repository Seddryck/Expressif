namespace Expressif.Observability;

internal sealed class ExpressionObservationScope : IExpressionObservation
{
    private readonly IExpressionObservation? observation;
    private int outcome;
    private int disposed;

    private ExpressionObservationScope(IExpressionObservation? observation)
        => this.observation = observation;

    public static ExpressionObservationScope Begin(
        IExpressionObserver observer,
        ExpressionObservationStage stage)
    {
        try
        {
            return new(observer.Begin(stage));
        }
        catch (Exception)
        {
            return new(null);
        }
    }

    public void Complete()
    {
        if (Interlocked.CompareExchange(ref outcome, 1, 0) != 0)
            return;

        try
        {
            observation?.Complete();
        }
        catch (Exception)
        {
            // Observation must not change the result of the observed operation.
        }
    }

    public void Fail(Exception exception)
    {
        if (Interlocked.CompareExchange(ref outcome, 2, 0) != 0)
            return;

        try
        {
            observation?.Fail(exception);
        }
        catch (Exception)
        {
            // Preserve the original operation exception.
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;

        try
        {
            observation?.Dispose();
        }
        catch (Exception)
        {
            // Observation must not change the result of the observed operation.
        }
    }
}
