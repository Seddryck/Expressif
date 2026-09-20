using Expressif.Library.Special;
using Expressif.Values;

namespace Expressif.Library.Coercions;

internal sealed class CoerceNumericDescriptor : CoercionDescriptor
{
    public CoerceNumericDescriptor()
        : base(
            "coerce-numeric",
            typeof(decimal?),
            CoercionDescriptorSupport.NumericSourceTypes
                .Concat([typeof(bool), typeof(string), typeof(OrderingValue)]),
            sourceType => CoercionDescriptorSupport.CreateNumericOrFallback(
                typeof(CoerceNumeric<>), sourceType, () => new CoerceNumeric())) { }
}
