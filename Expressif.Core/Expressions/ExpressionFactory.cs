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
            observer);

    private IExpressionParser Parser { get; }
    private IExpressionBinder Binder { get; }
    private IExpressionObserver? Observer { get; }

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
        var observation = ExpressionObservationScope.Create(Observer, ExpressionObservationStage.Parse);
        if (observation is null)
            return Parser.Parse(text);
        try
        {
            var syntax = Parser.Parse(text);
            ExpressionObservationScope.Complete(observation);
            return syntax;
        }
        catch (Exception exception)
        {
            ExpressionObservationScope.Fail(observation, exception);
            throw;
        }
        finally
        {
            ExpressionObservationScope.Dispose(observation);
        }
    }

    private IExpression ObserveBinding(Func<IExpression> bind)
    {
        var observation = ExpressionObservationScope.Create(Observer, ExpressionObservationStage.Bind);
        if (observation is null)
        {
            var direct = bind();
            return Observer is null ? direct : new ObservedExpression(direct, Observer);
        }
        try
        {
            var expression = bind();
            ExpressionObservationScope.Complete(observation);
            return new ObservedExpression(expression, Observer!);
        }
        catch (Exception exception)
        {
            ExpressionObservationScope.Fail(observation, exception);
            throw;
        }
        finally
        {
            ExpressionObservationScope.Dispose(observation);
        }
    }
}
