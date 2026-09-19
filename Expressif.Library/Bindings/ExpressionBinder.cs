using Expressif.Syntax;
using RuntimeExpression = Expressif.IExpression;
using RuntimeExpressionFactory = Expressif.Functions.FunctionFactory;

namespace Expressif.Bindings;

/// <summary>
/// Provides the standard syntax-to-runtime binder.
/// </summary>
public sealed class ExpressionBinder : IExpressionBinder
{
    private IContext Context { get; }
    private ExpressifBinder SyntaxBinder { get; }
    private RuntimeExpressionFactory RuntimeFactory { get; }

    public ExpressionBinder()
        : this(new Context()) { }

    public ExpressionBinder(IContext context)
        : this(context, new ExpressifBinder(), new RuntimeExpressionFactory()) { }

    internal ExpressionBinder(
        IContext context,
        ExpressifBinder syntaxBinder,
        RuntimeExpressionFactory runtimeFactory)
        => (Context, SyntaxBinder, RuntimeFactory) = (context, syntaxBinder, runtimeFactory);

    /// <summary>
    /// Binds syntax to an executable expression.
    /// </summary>
    public static RuntimeExpression Bind(RootExpressionSyntax syntax)
        => new ExpressionBinder().BindCore(syntax);

    /// <summary>
    /// Binds syntax to an executable expression that does not require input.
    /// </summary>
    public static RuntimeExpression BindClosed(RootExpressionSyntax syntax)
        => new ExpressionBinder().BindClosedCore(syntax);

    RuntimeExpression IExpressionBinder.Bind(RootExpressionSyntax syntax)
        => BindCore(syntax);

    RuntimeExpression IExpressionBinder.BindClosed(RootExpressionSyntax syntax)
        => BindClosedCore(syntax);

    private RuntimeExpression BindCore(RootExpressionSyntax syntax)
        => new Expressif.Expression(RuntimeFactory.Instantiate(SyntaxBinder.Bind(syntax), Context));

    private RuntimeExpression BindClosedCore(RootExpressionSyntax syntax)
        => new Expressif.Expression(RuntimeFactory.InstantiateClosed(SyntaxBinder.Bind(syntax), Context));
}
