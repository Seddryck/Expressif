using Expressif.Observability;

namespace Expressif;

/// <summary>
/// Holds state owned by one expression evaluation.
/// </summary>
internal sealed record EvaluationFrame
{
    public EvaluationFrame(object? current, object? ambient, IExpressionObservation? observation = null)
        => (Current, Ambient, Observation) = (current, ambient, observation);

    public object? Current { get; }
    public object? Ambient { get; }
    public IExpressionObservation? Observation { get; }
}
