using Expressif.Syntax;
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

    /// <summary>
    /// Binds a syntax tree while requiring it to be independent of pipeline input.
    /// </summary>
    /// <param name="syntax">The syntax tree to bind.</param>
    /// <returns>The bound executable expression.</returns>
    /// <remarks>
    /// Implementations should reject syntax whose evaluation requires a value supplied by the caller.
    /// </remarks>
    RuntimeExpression BindClosed(RootExpressionSyntax syntax);
}
