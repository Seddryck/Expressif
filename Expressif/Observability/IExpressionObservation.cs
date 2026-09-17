namespace Expressif.Observability;

/// <summary>
/// Represents the state of one observed expression lifecycle operation.
/// </summary>
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
