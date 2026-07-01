
using Expressif.Functions;
using Expressif.Accumulators;

namespace Expressif.Functions.Array;

/// <summary>
/// Base implementation for array aggregation functions.
/// It iterates over an enumerable input and delegates state management to an accumulator.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
public abstract class BaseAggregationFunction : IFunction
{
    public object? Evaluate(object? value)
    {
        if (!AggregationEnumerable.TryGetEnumerable(value, out var enumerable))
            return null;

        var accumulator = InstantiateAccumulator();
        accumulator.Initialize();
        foreach (var item in enumerable!)
            accumulator.Accumulate(item);

        return accumulator.GetValue();
    }

    protected abstract IAccumulator InstantiateAccumulator();
}

/// <summary>
/// Returns the number of items in the input enumerable.
/// Returns `null` when the input is not an enumerable or is a string.
/// </summary>
public class Count : BaseAggregationFunction
{
    protected override IAccumulator InstantiateAccumulator()
        => new CountAccumulator();
}

/// <summary>
/// Returns the sum of all numeric-convertible items in the input enumerable.
/// Returns `0` for an empty enumerable and `null` when the input is not an enumerable or is a string.
/// </summary>
public class Sum : BaseAggregationFunction
{
    protected override IAccumulator InstantiateAccumulator()
        => new SumAccumulator();
}

/// <summary>
/// Returns the minimum value among numeric-convertible items in the input enumerable.
/// Returns `null` for an empty enumerable, and `null` when the input is not an enumerable or is a string.
/// </summary>
public class Min : BaseAggregationFunction
{
    protected override IAccumulator InstantiateAccumulator()
        => new MinAccumulator();
}

/// <summary>
/// Returns the maximum value among numeric-convertible items in the input enumerable.
/// Returns `null` for an empty enumerable, and `null` when the input is not an enumerable or is a string.
/// </summary>
public class Max : BaseAggregationFunction
{
    protected override IAccumulator InstantiateAccumulator()
        => new MaxAccumulator();
}

/// <summary>
/// Returns the first item of the input enumerable.
/// Returns `null` for an empty enumerable, and `null` when the input is not an enumerable or is a string.
/// </summary>
public class First : BaseAggregationFunction
{
    protected override IAccumulator InstantiateAccumulator()
        => new FirstAccumulator();
}

/// <summary>
/// Returns the last item of the input enumerable.
/// Returns `null` for an empty enumerable, and `null` when the input is not an enumerable or is a string.
/// </summary>
public class Last : BaseAggregationFunction
{
    protected override IAccumulator InstantiateAccumulator()
        => new LastAccumulator();
}
