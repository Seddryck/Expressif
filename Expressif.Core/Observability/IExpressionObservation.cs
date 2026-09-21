namespace Expressif.Observability;

/// <summary>
/// Represents the state of one observed expression lifecycle operation.
/// </summary>
/// <remarks>
/// Exceptions thrown by <see cref="Complete"/>, <see cref="Fail"/>, or <see cref="IDisposable.Dispose"/>
/// are suppressed and do not change the result or exception of the operation being observed.
/// </remarks>
public interface IExpressionObservation : IDisposable
{
    /// <summary>
    /// Signals that the operation completed successfully.
    /// </summary>
    void Complete() { }

    /// <summary>
    /// Signals that the operation failed.
    /// </summary>
    void Fail(Exception exception) { }
}
