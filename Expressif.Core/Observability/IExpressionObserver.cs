namespace Expressif.Observability;

/// <summary>
/// Observes expression lifecycle operations.
/// </summary>
/// <remarks>
/// An observer can be shared by bound expressions and must be safe for concurrent calls.
/// Every call to <see cref="Begin"/> must return a distinct observation dedicated to that operation.
/// Multiple observations returned by the same observer can be active concurrently, but each observation
/// is used only by the operation for which it was created.
/// Exceptions thrown by an observer are suppressed and are not themselves observed. They do not
/// change the result or exception of the parsing, binding, or evaluation operation being observed.
/// </remarks>
public interface IExpressionObserver
{
    /// <summary>
    /// Begins observing one parse, bind, or evaluation operation.
    /// </summary>
    /// <remarks>
    /// If this method throws, the expression operation proceeds without an observation.
    /// </remarks>
    /// <param name="stage">The lifecycle stage performed by the operation.</param>
    /// <returns>
    /// A dedicated observation that receives the operation outcome and is disposed when the operation ends.
    /// </returns>
    IExpressionObservation Begin(ExpressionObservationStage stage);
}
