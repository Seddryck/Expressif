using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;

namespace Expressif.Functions;

public class ChainFunction : IFunction
{
    internal IEnumerable<IFunction> Functions { get; }

    public ChainFunction(IEnumerable<IFunction> functions)
        => Functions = functions;

    public virtual object? Evaluate(object? value)
        => Functions.Aggregate(value, (v, func) => func.Evaluate(v));
}

public sealed class ChainFunction<TIn, TOut> : ChainFunction, IFunction<TIn, TOut>
{
    private Func<TIn, TOut> Pipeline { get; }

    public ChainFunction(IEnumerable<IFunction> functions, Func<TIn, TOut> pipeline)
        : base(functions)
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
