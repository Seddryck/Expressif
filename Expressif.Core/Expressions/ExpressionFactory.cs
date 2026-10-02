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
        : this(binder, parser, observer, null) { }

    private ExpressionFactory(
        IExpressionBinder binder,
        IExpressionParser? parser,
        IExpressionObserver? observer,
        IEnumerable<IFunctionObserver>? functionObservers)
        => (Parser, Binder, Observer, FunctionObservers) = (
            parser ?? new ExpressionParser(),
            binder ?? throw new ArgumentNullException(nameof(binder)),
            observer ?? NoOpExpressionObserver.Instance,
            functionObservers?.ToArray() ?? []);

    private IExpressionParser Parser { get; }
    private IExpressionBinder Binder { get; }
    private IExpressionObserver Observer { get; }
    private IReadOnlyList<IFunctionObserver> FunctionObservers { get; }

    /// <summary>
    /// Returns a factory that passively notifies the supplied observers at every bound-function boundary.
    /// </summary>
    public ExpressionFactory WithFunctionObservers(IEnumerable<IFunctionObserver> observers)
        => new(Binder, Parser, Observer, observers ?? throw new ArgumentNullException(nameof(observers)));

    public IExpression Create(string text)
        => Create(Parse(text));

    public IExpression Create(RootExpressionSyntax syntax)
        => ObserveBinding(() => Binder.Bind(syntax, FunctionObservers));

    public IExpression CreateClosed(string text)
        => CreateClosed(Parse(text));

    public IExpression CreateClosed(RootExpressionSyntax syntax)
        => ObserveBinding(() => Binder.BindClosed(syntax, FunctionObservers));

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
