using System.Diagnostics.CodeAnalysis;

namespace Expressif.Values.Casters;

public sealed class OrderingCaster : ICaster<OrderingValue>
{
    public bool TryCast(object obj, [NotNullWhen(true)] out OrderingValue? value)
    {
        value = null;
        if (!NumericCoercion.IsSupported(obj.GetType())
            || !NumericCoercion.TryToDecimal(obj, out var numeric))
            return false;

        value = numeric switch
        {
            -1 => OrderingValue.Less,
            0 => OrderingValue.Equal,
            1 => OrderingValue.Greater,
            _ => null,
        };
        return value is not null;
    }

    public OrderingValue Cast(object obj)
        => TryCast(obj, out var value)
            ? value
            : throw new InvalidCastException(
                $"Cannot cast an object of type '{obj.GetType().FullName}' to an ordering value.");
}
