namespace Expressif.Functions;

public abstract class Function<TIn, TOut> : IFunction<TIn, TOut>
{
    public abstract TOut Evaluate(TIn value);

    object? IFunction.Evaluate(object? value)
    {
        if (value is TIn typed)
            return Evaluate(typed);

        return value is null && default(TIn) is null
            ? Evaluate(default!)
            : null;
    }
}
