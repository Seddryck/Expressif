namespace Expressif.Observability;

/// <summary>
/// Observes expression lifecycle operations.
/// </summary>
/// <remarks>
/// Implementations are shared by bound expressions and must be safe for concurrent use.
/// Each call must return an observation that belongs only to that operation.
/// </remarks>
public interface IExpressionObserver
{
    IExpressionObservation Begin(ExpressionObservationStage stage);
}
