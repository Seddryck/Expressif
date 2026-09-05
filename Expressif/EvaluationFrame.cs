using Expressif.Observability;

namespace Expressif;

/// <summary>
/// Holds state owned by one expression evaluation.
/// </summary>
internal sealed record EvaluationFrame
{
    public EvaluationFrame(
        object? current,
        object? ambient,
        IExpressionObservation? observation = null,
        EvaluationFrame? parent = null)
        => (Current, Ambient, Observation, Parent) = (current, ambient, observation, parent);

    public object? Current { get; }
    public object? Ambient { get; }
    public IExpressionObservation? Observation { get; }
    public EvaluationFrame? Parent { get; }
}
