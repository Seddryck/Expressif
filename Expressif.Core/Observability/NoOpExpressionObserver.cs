namespace Expressif.Observability;

internal sealed class NoOpExpressionObserver : IExpressionObserver
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
