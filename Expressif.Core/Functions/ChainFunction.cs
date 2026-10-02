using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;
using Expressif.Observability;

namespace Expressif.Functions;

internal class ChainFunction : IFunction
{
    private readonly FunctionObservationContext[]? observationContexts;
    internal IEnumerable<IFunction> Functions { get; }

    public ChainFunction(IEnumerable<IFunction> functions)
        => Functions = functions;

    public ChainFunction(
        IEnumerable<IFunction> functions,
        FunctionObservationContext[] observationContexts)
        => (Functions, this.observationContexts) = (functions, observationContexts);

    public virtual object? Evaluate(object? value)
    {
        var index = 0;
        foreach (var function in Functions)
        {
            if (function is IPipelineControlFunction control)
            {
                value = observationContexts is null
                    ? control.Evaluate(value, out var terminate)
                    : FunctionObservationDispatcher.EvaluateControl(control, observationContexts[index], value, out terminate);
                if (terminate)
                    return value;
            }
            else
            {
                value = observationContexts is null
                    ? function.Evaluate(value)
                    : FunctionObservationDispatcher.Evaluate(function, observationContexts[index], value);
            }
            index++;
        }
        return value;
    }
}

internal sealed class ChainFunction<TIn, TOut> : ChainFunction, IFunction<TIn, TOut>
{
    private Func<TIn, TOut> Pipeline { get; }

    public ChainFunction(IEnumerable<IFunction> functions, Func<TIn, TOut> pipeline)
        : base(functions)
        => Pipeline = pipeline;

    public ChainFunction(
        IEnumerable<IFunction> functions,
        Func<TIn, TOut> pipeline,
        FunctionObservationContext[] observationContexts)
        : base(functions, observationContexts)
        => Pipeline = pipeline;

    public TOut Evaluate(TIn value) => Pipeline.Invoke(value);

    public override object? Evaluate(object? value)
    {
        if (value is TIn typed)
            return Evaluate(typed);

        return value is null && default(TIn) is null
            ? Evaluate(default!)
            : base.Evaluate(value);
    }
}
