using Expressif.Functions;
using System.Collections;

namespace Expressif.Functions.Array;

public abstract class BaseArrayFunction : IFunction
{
    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        return EvaluateArray(enumerable!);
    }

    protected abstract object? EvaluateArray(IEnumerable enumerable);
}
