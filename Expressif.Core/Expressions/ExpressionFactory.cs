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
        IExpressionBinder binder,
        IExpressionParser? parser = null,
        IExpressionObserver? observer = null)
        => (Parser, Binder, Observer) = (
            parser ?? new ExpressionParser(),
            binder ?? throw new ArgumentNullException(nameof(binder)),
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
        using var observation = ExpressionObservationScope.Begin(Observer, ExpressionObservationStage.Parse);
        try
        {
            var syntax = Parser.Parse(text);
            observation.Complete();
            return syntax;
        }
        catch (Exception exception)
        {
            observation.Fail(exception);
            throw;
        }
    }

    private IExpression ObserveBinding(Func<IExpression> bind)
    {
        IExpression expression;
        using (var observation = ExpressionObservationScope.Begin(Observer, ExpressionObservationStage.Bind))
        {
            try
            {
                expression = bind();
                observation.Complete();
            }
            catch (Exception exception)
            {
                observation.Fail(exception);
                throw;
            }
        }

        return ReferenceEquals(Observer, NoOpExpressionObserver.Instance)
            ? expression
            : new ObservedExpression(expression, Observer);
    }
}
