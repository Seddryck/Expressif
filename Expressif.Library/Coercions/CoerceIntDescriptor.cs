using Expressif.Library.Special;
using Expressif.Values;

namespace Expressif.Library.Coercions;

internal sealed class CoerceIntDescriptor : CoercionDescriptor
{
    public CoerceIntDescriptor()
        : base(
            "coerce-int",
            typeof(int?),
            CoercionDescriptorSupport.NumericSourceTypes
                .Concat([typeof(bool), typeof(string), typeof(OrderingValue)]),
            sourceType => CoercionDescriptorSupport.GetNumericOrFallbackType(
                typeof(CoerceInt<>), typeof(CoerceInt), sourceType),
            sourceType => CoercionDescriptorSupport.CreateNumericOrFallback(
                typeof(CoerceInt<>), sourceType, () => new CoerceInt())) { }
}
