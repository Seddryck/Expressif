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
    /// <summary>
    /// Begins observing one parse, bind, or evaluation operation.
    /// </summary>
    /// <param name="stage">The lifecycle stage performed by the operation.</param>
    /// <returns>
    /// A dedicated observation that receives the operation outcome and is disposed when the operation ends.
    /// </returns>
    IExpressionObservation Begin(ExpressionObservationStage stage);
}
