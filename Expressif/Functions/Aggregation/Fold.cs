using Expressif.Functions;
using System;

namespace Expressif.Functions.Array;

[Function]
public class Fold : IFunction
{
    public Func<IAccumulator> Accumulator { get; }

    public Fold(Func<IAccumulator> accumulator)
        => Accumulator = accumulator;

    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var accumulator = Accumulator.Invoke();
        accumulator.Initialize();
        foreach (var item in enumerable!)
            accumulator.Accumulate(item);

        return accumulator.GetValue();
    }
}
