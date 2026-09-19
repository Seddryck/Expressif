using System.Collections;
using Expressif.Functions;

namespace Expressif.Functions.Array;

public abstract class BaseArrayFunction<TOut> : IFunction<IEnumerable, TOut?>
{
    TOut? IFunction<IEnumerable, TOut?>.Evaluate(IEnumerable value)
        => EvaluateArray(value) is TOut result ? result : default;

    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        return EvaluateArray(enumerable!);
    }

    protected abstract object? EvaluateArray(IEnumerable enumerable);
}

public abstract class BaseArrayFunction : BaseArrayFunction<IEnumerable>
{ }
