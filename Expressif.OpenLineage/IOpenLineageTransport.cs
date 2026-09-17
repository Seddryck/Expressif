namespace Expressif.OpenLineage;

/// <summary>
/// Publishes a serialized OpenLineage run event.
/// </summary>
public interface IOpenLineageTransport
{
    void Emit(string json);
}
