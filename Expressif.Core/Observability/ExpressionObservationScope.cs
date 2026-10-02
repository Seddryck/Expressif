namespace Expressif.Observability;

internal static class ExpressionObservationScope
{
    public static IExpressionObservation? Create(
        IExpressionObserver? observer,
        ExpressionObservationStage stage)
    {
        if (observer is null)
            return null;
        try
        {
            return observer.Create(stage);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void Complete(IExpressionObservation observation)
    {
        try
        {
            observation.Complete();
        }
        catch (Exception)
        {
            // Observation must not change the result of the observed operation.
        }
    }

    public static void Fail(IExpressionObservation observation, Exception exception)
    {
        try
        {
            observation.Fail(exception);
        }
        catch (Exception)
        {
            // Preserve the original operation exception.
        }
    }

    public static void Dispose(IExpressionObservation observation)
    {
        try
        {
            observation.Dispose();
        }
        catch (Exception)
        {
            // Observation must not change the result of the observed operation.
        }
    }
}
