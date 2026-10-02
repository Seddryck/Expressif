namespace Expressif.Observability;

/// <summary>
/// Identifies an observable stage in the expression lifecycle.
/// </summary>
public enum ExpressionObservationStage
{
    /// <summary>
    /// Parsing source text into a syntax tree.
    /// </summary>
    Parse,

    /// <summary>
    /// Binding a syntax tree to an executable expression.
    /// </summary>
    Bind,

    /// <summary>
    /// Evaluating a bound expression.
    /// </summary>
    Evaluate,
}
