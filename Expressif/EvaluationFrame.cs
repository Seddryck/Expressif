using Expressif.Observability;
using Expressif.Semantics;

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
        => (Scope, Observation, Parent) = (new(current, ambient, parent?.Scope), observation, parent);

    internal EvaluationFrame(ScopeFrame<object?> scope, EvaluationFrame parent)
        => (Scope, Parent) = (scope, parent);

    internal ScopeFrame<object?> Scope { get; }
    public object? Current => Scope.Current;
    public object? Ambient => Scope.Root;
    public IExpressionObservation? Observation { get; }
    public EvaluationFrame? Parent { get; }
}
