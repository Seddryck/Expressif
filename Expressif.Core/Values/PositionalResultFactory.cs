using Expressif.Values.Casters;

namespace Expressif.Values;

internal static class PositionalResultFactory
{
    public static TupleValue? Create(IPositionalValue source, object?[] values)
        => source is VectorValue
            ? values.All(value => value is not null && TypeChecker.IsNumericType(value))
                ? new Vector(values)
                : null
            : new Tuple(values);
}
