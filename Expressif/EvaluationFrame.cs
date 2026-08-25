using Expressif.Observability;

namespace Expressif;

/// <summary>
/// Holds state owned by one expression evaluation.
/// </summary>
internal sealed record EvaluationFrame(object? Input, IExpressionObservation Observation);
