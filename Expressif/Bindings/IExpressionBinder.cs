using Expressif.Syntax;
using RuntimeExpression = Expressif.IExpression;

namespace Expressif.Bindings;

/// <summary>
/// Binds syntax to an executable expression.
/// </summary>
public interface IExpressionBinder
{
    RuntimeExpression Bind(RootExpressionSyntax syntax);
}
