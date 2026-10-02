using Expressif.Syntax;
using Expressif.Planning;
using Expressif.Observability;
using RuntimeExpression = Expressif.IExpression;

namespace Expressif.Bindings;

/// <summary>
/// Binds syntax to an executable expression.
/// </summary>
public interface IExpressionBinder
{
    /// <summary>
    /// Binds a syntax tree to an executable expression, allowing the tree to consume pipeline input.
    /// </summary>
    /// <param name="syntax">The syntax tree to bind.</param>
    /// <returns>The bound executable expression.</returns>
    RuntimeExpression Bind(RootExpressionSyntax syntax);

    RuntimeExpression Bind(RootExpressionSyntax syntax, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? Bind(syntax) : throw new NotSupportedException("This binder does not support function observation.");

    /// <summary>
    /// Binds a logical plan to an executable expression, allowing the plan to consume pipeline input.
    /// </summary>
    /// <param name="plan">The logical plan to bind.</param>
    /// <returns>The bound executable expression.</returns>
    RuntimeExpression Bind(LogicalPlan plan);

    RuntimeExpression Bind(LogicalPlan plan, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? Bind(plan) : throw new NotSupportedException("This binder does not support function observation.");

    /// <summary>
    /// Binds a syntax tree while requiring it to be independent of pipeline input.
    /// </summary>
    /// <param name="syntax">The syntax tree to bind.</param>
    /// <returns>The bound executable expression.</returns>
    /// <remarks>
    /// Implementations should reject syntax whose evaluation requires a value supplied by the caller.
    /// </remarks>
    RuntimeExpression BindClosed(RootExpressionSyntax syntax);

    RuntimeExpression BindClosed(RootExpressionSyntax syntax, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? BindClosed(syntax) : throw new NotSupportedException("This binder does not support function observation.");

    /// <summary>
    /// Binds a logical plan while requiring it to be independent of pipeline input.
    /// </summary>
    /// <param name="plan">The logical plan to bind.</param>
    /// <returns>The bound executable expression.</returns>
    /// <remarks>
    /// Implementations should reject plans whose evaluation requires a value supplied by the caller.
    /// </remarks>
    RuntimeExpression BindClosed(LogicalPlan plan);

    RuntimeExpression BindClosed(LogicalPlan plan, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? BindClosed(plan) : throw new NotSupportedException("This binder does not support function observation.");
}
