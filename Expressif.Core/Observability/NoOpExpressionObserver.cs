namespace Expressif.Observability;

internal sealed class NoOpExpressionObserver : IExpressionObserver
{
    /// <summary>
    /// Gets the shared observer instance.
    /// </summary>
    /// <value>The singleton observer that discards lifecycle notifications.</value>
    public static NoOpExpressionObserver Instance { get; } = new();

    private NoOpExpressionObserver() { }

    /// <summary>
    /// Begins an observation that discards all lifecycle notifications.
    /// </summary>
    /// <param name="stage">The lifecycle stage, which is ignored.</param>
    /// <returns>An observation that performs no work.</returns>
    public IExpressionObservation Begin(ExpressionObservationStage stage)
        => NoOpExpressionObservation.Instance;

    private sealed class NoOpExpressionObservation : IExpressionObservation
    {
        public static NoOpExpressionObservation Instance { get; } = new();

        public void Dispose() { }
    }
}
