using Expressif.Bindings;
using Expressif.Observability;
using Expressif.Syntax;

namespace Expressif;

/// <summary>
/// Composes parsing and binding into executable expression creation.
/// </summary>
public sealed class ExpressionFactory
{
    public ExpressionFactory(
        IExpressionParser? parser = null,
        IExpressionBinder? binder = null,
        IExpressionObserver? observer = null)
        => (Parser, Binder, Observer) = (
            parser ?? new ExpressionParser(),
            binder ?? new ExpressionBinder(),
            observer ?? NoOpExpressionObserver.Instance);

    private IExpressionParser Parser { get; }
    private IExpressionBinder Binder { get; }
    private IExpressionObserver Observer { get; }

    public IExpression Create(string text)
        => Create(Parse(text));

    public IExpression Create(RootExpressionSyntax syntax)
        => ObserveBinding(() => Binder.Bind(syntax));

    public IExpression CreateClosed(string text)
        => CreateClosed(Parse(text));

    public IExpression CreateClosed(RootExpressionSyntax syntax)
        => ObserveBinding(() => Binder.BindClosed(syntax));

    private RootExpressionSyntax Parse(string text)
    {
        using var observation = Observer.Begin(ExpressionObservationStage.Parse);
        return Parser.Parse(text);
    }

    private IExpression ObserveBinding(Func<IExpression> bind)
    {
        IExpression expression;
        using (Observer.Begin(ExpressionObservationStage.Bind))
            expression = bind();

        return ReferenceEquals(Observer, NoOpExpressionObserver.Instance)
            ? expression
            : new ObservedExpression(expression, Observer);
    }
}
