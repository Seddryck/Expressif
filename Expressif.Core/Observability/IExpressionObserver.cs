namespace Expressif.Observability;

/// <summary>
/// Observes expression lifecycle operations.
/// </summary>
/// <remarks>
/// An observer can be shared by bound expressions and must be safe for concurrent calls.
/// Every non-null result from <see cref="Create"/> must be a distinct observation dedicated to that operation.
/// Multiple observations returned by the same observer can be active concurrently, but each observation
/// is used only by the operation for which it was created.
/// Exceptions thrown by an observer are suppressed and are not themselves observed. They do not
/// change the result or exception of the parsing, binding, or evaluation operation being observed.
/// </remarks>
public interface IExpressionObserver
{
    /// <summary>
    /// Creates an observation for one parse, bind, or evaluation operation.
    /// </summary>
    /// <param name="stage">The lifecycle stage performed by the operation.</param>
    /// <returns>
    /// A dedicated observation, or <see langword="null"/> when this observer does not observe the stage.
    /// </returns>
    /// <remarks>
    /// If this method throws, the expression operation proceeds without an observation.
    /// </remarks>
    IExpressionObservation? Create(ExpressionObservationStage stage);
}
