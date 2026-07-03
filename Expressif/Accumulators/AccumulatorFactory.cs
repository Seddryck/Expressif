using System;

namespace Expressif.Accumulators;

internal static class AccumulatorFactory
{
    public static IAccumulator Instantiate(string? name)
    {
        var key = (name ?? string.Empty).Trim();
        var typeName = $"{key}Accumulator";
        var fullName = $"{typeof(AccumulatorFactory).Namespace}.{typeName}";

        var type = typeof(AccumulatorFactory).Assembly.GetType(fullName, false, true);
        if (type is not null
            && typeof(IAccumulator).IsAssignableFrom(type)
            && type.GetConstructor(Type.EmptyTypes) is not null)
            return (IAccumulator)Activator.CreateInstance(type)!;

        throw new NotImplementedFunctionException(name ?? string.Empty);
    }
}
