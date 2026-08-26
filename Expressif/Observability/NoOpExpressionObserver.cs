namespace Expressif.Observability;

/// <summary>
/// Provides observation scopes that perform no work.
/// </summary>
public sealed class NoOpExpressionObserver : IExpressionObserver
{
    public static NoOpExpressionObserver Instance { get; } = new();

    private NoOpExpressionObserver() { }

    public IExpressionObservation Begin(ExpressionObservationStage stage)
        => NoOpExpressionObservation.Instance;

    private sealed class NoOpExpressionObservation : IExpressionObservation
    {
        public static NoOpExpressionObservation Instance { get; } = new();

        public void Dispose() { }
    }
}
