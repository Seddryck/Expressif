using Expressif.Functions;

namespace Expressif.Observability;

internal static class FunctionObservationDispatcher
{
    public static object? Evaluate(
        IFunction function,
        FunctionObservationContext context,
        IFunctionObserver[] observers,
        object? input)
    {
        try
        {
            var output = function.Evaluate(input);
            Complete(observers, context, input, output);
            return output;
        }
        catch (Exception exception)
        {
            Fail(observers, context, input, exception);
            throw;
        }
    }

    public static TOut Evaluate<TIn, TOut>(
        IFunction<TIn, TOut> function,
        FunctionObservationContext context,
        IFunctionObserver[] observers,
        TIn input)
    {
        try
        {
            var output = function.Evaluate(input);
            Complete(observers, context, input, output);
            return output;
        }
        catch (Exception exception)
        {
            Fail(observers, context, input, exception);
            throw;
        }
    }

    public static object? EvaluateControl(
        IPipelineControlFunction function,
        FunctionObservationContext context,
        IFunctionObserver[] observers,
        object? input,
        out bool terminate)
    {
        try
        {
            var output = function.Evaluate(input, out terminate);
            Complete(observers, context, input, output);
            return output;
        }
        catch (Exception exception)
        {
            Fail(observers, context, input, exception);
            throw;
        }
    }

    private static void Complete(
        IFunctionObserver[] observers,
        FunctionObservationContext context,
        object? input,
        object? output)
    {
        foreach (var observer in observers)
        {
            try { observer.OnCompleted(context, input, output); }
            catch (Exception) { }
        }
    }

    private static void Fail(
        IFunctionObserver[] observers,
        FunctionObservationContext context,
        object? input,
        Exception exception)
    {
        foreach (var observer in observers)
        {
            try { observer.OnFailed(context, input, exception); }
            catch (Exception) { }
        }
    }
}
