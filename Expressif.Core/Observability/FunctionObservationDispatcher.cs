using Expressif.Functions;

namespace Expressif.Observability;

internal static class FunctionObservationDispatcher
{
    public static object? Evaluate(
        IFunction function,
        FunctionObservationContext context,
        object? input)
    {
        if (!EvaluationRuntime.HasDetailedObservations)
            return function.Evaluate(input);
        using var scope = EvaluationRuntime.EnterFunction(context);
        try
        {
            var output = function.Evaluate(input);
            EvaluationRuntime.ReportFunctionCompleted(context, input, output);
            return output;
        }
        catch (Exception exception)
        {
            EvaluationRuntime.ReportFunctionFailed(context, input, exception);
            throw;
        }
    }

    public static TOut Evaluate<TIn, TOut>(
        IFunction<TIn, TOut> function,
        FunctionObservationContext context,
        TIn input)
    {
        if (!EvaluationRuntime.HasDetailedObservations)
            return function.Evaluate(input);
        using var scope = EvaluationRuntime.EnterFunction(context);
        try
        {
            var output = function.Evaluate(input);
            EvaluationRuntime.ReportFunctionCompleted(context, input, output);
            return output;
        }
        catch (Exception exception)
        {
            EvaluationRuntime.ReportFunctionFailed(context, input, exception);
            throw;
        }
    }

    public static object? EvaluateControl(
        IPipelineControlFunction function,
        FunctionObservationContext context,
        object? input,
        out bool terminate)
    {
        if (!EvaluationRuntime.HasDetailedObservations)
            return function.Evaluate(input, out terminate);
        using var scope = EvaluationRuntime.EnterFunction(context);
        try
        {
            var output = function.Evaluate(input, out terminate);
            EvaluationRuntime.ReportFunctionCompleted(context, input, output);
            return output;
        }
        catch (Exception exception)
        {
            EvaluationRuntime.ReportFunctionFailed(context, input, exception);
            throw;
        }
    }
}
