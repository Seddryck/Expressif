using Expressif.Bindings;
using Expressif.Syntax;

namespace Expressif;

/// <summary>
/// Composes parsing and binding into executable expression creation.
/// </summary>
public sealed class ExpressionFactory
{
    public ExpressionFactory()
        : this(new ExpressionParser(), new ExpressionBinder()) { }

    public ExpressionFactory(IExpressionParser? parser = null, IExpressionBinder? binder = null)
        => (Parser, Binder) = (parser ?? new ExpressionParser(), binder ?? new ExpressionBinder());

    private IExpressionParser Parser { get; }
    private IExpressionBinder Binder { get; }

    public IExpression Create(string text)
        => Create(Parser.Parse(text));

    public IExpression Create(RootExpressionSyntax syntax)
        => Binder.Bind(syntax);
}
