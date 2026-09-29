namespace Expressif.Observability;

/// <summary>
/// Identifies an observable stage in the expression lifecycle.
/// </summary>
public enum ExpressionObservationStage
{
    Parse,
    Bind,
    Evaluate,
}
