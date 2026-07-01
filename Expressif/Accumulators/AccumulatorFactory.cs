using System;

namespace Expressif.Accumulators;

internal static class AccumulatorFactory
{
    public static IAccumulator Instantiate(string? name)
    {
        var key = (name ?? string.Empty).Trim();

        if (key.Equals("count", StringComparison.OrdinalIgnoreCase)) return new CountAccumulator();
        if (key.Equals("sum", StringComparison.OrdinalIgnoreCase)) return new SumAccumulator();
        if (key.Equals("min", StringComparison.OrdinalIgnoreCase)) return new MinAccumulator();
        if (key.Equals("max", StringComparison.OrdinalIgnoreCase)) return new MaxAccumulator();
        if (key.Equals("first", StringComparison.OrdinalIgnoreCase)) return new FirstAccumulator();
        if (key.Equals("last", StringComparison.OrdinalIgnoreCase)) return new LastAccumulator();

        throw new NotImplementedFunctionException(name ?? string.Empty);
    }
}
