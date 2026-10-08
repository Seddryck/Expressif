namespace Expressif.Observability;

/// <summary>Composes expression observers while preserving registration order.</summary>
public static class ExpressionObservers
{
    public static IExpressionObserver? Combine(params IExpressionObserver[] observers)
    {
        ArgumentNullException.ThrowIfNull(observers);
        if (observers.Any(observer => observer is null))
            throw new ArgumentException("Observers must not contain null entries.", nameof(observers));
        return observers.Length switch
        {
            0 => null,
            1 => observers[0],
            _ => new CompositeExpressionObserver(observers),
        };
    }
}

internal sealed class CompositeExpressionObserver(IExpressionObserver[] observers) : IExpressionObserver
{
    public IExpressionObservation? Create(ExpressionObservationStage stage)
    {
        var observations = new List<IExpressionObservation>(observers.Length);
        foreach (var observer in observers)
        {
            try
            {
                if (observer.Create(stage) is { } observation)
                    observations.Add(observation);
            }
            catch (Exception) { }
        }
        return CompositeExpressionObservationFactory.Create(observations);
    }
}

internal interface ICompositeExpressionObservation
{
    IReadOnlyList<IExpressionObservation> Children { get; }
}

internal static class CompositeExpressionObservationFactory
{
    public static IExpressionObservation? Create(IReadOnlyList<IExpressionObservation> observations)
    {
        if (observations.Count == 0)
            return null;
        if (observations.Count == 1)
            return observations[0];
        var functions = observations.OfType<IFunctionObserver>().ToArray();
        var flows = observations.OfType<IFlowObserver>().ToArray();
        return (functions.Length > 0, flows.Length > 0) switch
        {
            (true, true) => new FullCompositeObservation(observations, functions, flows),
            (true, false) => new FunctionCompositeObservation(observations, functions),
            (false, true) => new FlowCompositeObservation(observations, flows),
            _ => new CompositeObservation(observations),
        };
    }
}

internal class CompositeObservation(IReadOnlyList<IExpressionObservation> children)
    : IExpressionObservation, ICompositeExpressionObservation
{
    public IReadOnlyList<IExpressionObservation> Children { get; } = children;

    public void Complete()
    {
        foreach (var child in Children)
        {
            try { child.Complete(); }
            catch (Exception) { }
        }
    }

    public void Fail(Exception exception)
    {
        foreach (var child in Children)
        {
            try { child.Fail(exception); }
            catch (Exception) { }
        }
    }

    public void Dispose()
    {
        for (var index = Children.Count - 1; index >= 0; index--)
        {
            try { Children[index].Dispose(); }
            catch (Exception) { }
        }
    }
}

internal sealed class FunctionCompositeObservation(
    IReadOnlyList<IExpressionObservation> children,
    IFunctionObserver[] observers) : CompositeObservation(children), IFunctionObserver
{
    public void OnCompleted(FunctionObservationContext context, object? input, object? output)
    {
        foreach (var observer in observers)
        {
            try { observer.OnCompleted(context, input, output); }
            catch (Exception) { }
        }
    }

    public void OnFailed(FunctionObservationContext context, object? input, Exception exception)
    {
        foreach (var observer in observers)
        {
            try { observer.OnFailed(context, input, exception); }
            catch (Exception) { }
        }
    }
}

internal sealed class FlowCompositeObservation(
    IReadOnlyList<IExpressionObservation> children,
    IFlowObserver[] observers) : CompositeObservation(children), IFlowObserver
{
    public void OnDecision(FunctionObservationContext function, FlowDecision decision)
    {
        foreach (var observer in observers)
        {
            try { observer.OnDecision(function, decision); }
            catch (Exception) { }
        }
    }
}

internal sealed class FullCompositeObservation(
    IReadOnlyList<IExpressionObservation> children,
    IFunctionObserver[] functionObservers,
    IFlowObserver[] flowObservers) : CompositeObservation(children), IFunctionObserver, IFlowObserver
{
    public void OnCompleted(FunctionObservationContext context, object? input, object? output)
    {
        foreach (var observer in functionObservers)
        {
            try { observer.OnCompleted(context, input, output); }
            catch (Exception) { }
        }
    }

    public void OnFailed(FunctionObservationContext context, object? input, Exception exception)
    {
        foreach (var observer in functionObservers)
        {
            try { observer.OnFailed(context, input, exception); }
            catch (Exception) { }
        }
    }

    public void OnDecision(FunctionObservationContext function, FlowDecision decision)
    {
        foreach (var observer in flowObservers)
        {
            try { observer.OnDecision(function, decision); }
            catch (Exception) { }
        }
    }
}
