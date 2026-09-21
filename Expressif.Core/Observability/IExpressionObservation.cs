namespace Expressif.Observability;

/// <summary>
/// Represents the state of one observed expression lifecycle operation.
/// </summary>
/// <remarks>
/// The observation is owned by one parsing, binding, or evaluation operation and is not used concurrently.
/// Exactly one of <see cref="Complete"/> or <see cref="Fail"/> is called. The terminal callback is followed
/// by <see cref="IDisposable.Dispose"/> before the operation returns or rethrows its original exception.
/// Other observations created by the same observer can be active concurrently.
/// Exceptions thrown by <see cref="Complete"/>, <see cref="Fail"/>, or <see cref="IDisposable.Dispose"/>
/// are suppressed and do not change the result or exception of the operation being observed.
/// </remarks>
public interface IExpressionObservation : IDisposable
{
    /// <summary>
    /// Signals that the operation completed successfully, before this observation is disposed.
    /// </summary>
    void Complete() { }

    /// <summary>
    /// Signals that the operation failed, before this observation is disposed.
    /// </summary>
    void Fail(Exception exception) { }
}
