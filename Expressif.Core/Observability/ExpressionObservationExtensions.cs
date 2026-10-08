namespace Expressif.Observability;

/// <summary>Provides scoped runtime activation for evaluation observations.</summary>
public static class ExpressionObservationExtensions
{
    /// <summary>Makes an observation current until the returned scope is disposed.</summary>
    /// <remarks>Disposing the activation does not complete, fail, or dispose the observation.</remarks>
    public static IDisposable Activate(this IExpressionObservation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        return EvaluationRuntime.ActivateObservation(observation);
    }
}
