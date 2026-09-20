using Expressif.Library.Special;
using Expressif.Values;

namespace Expressif.Library.Coercions;

internal sealed class CoerceTextDescriptor : CoercionDescriptor
{
    public CoerceTextDescriptor()
        : base(
            "coerce-text",
            typeof(string),
            CoercionDescriptorSupport.NumericSourceTypes.Concat(
                [typeof(string), typeof(bool), typeof(DateOnly), typeof(DateTime), typeof(YearMonth)]),
            sourceType => CoercionDescriptorSupport.CreateNumericOrFallback(
                typeof(CoerceText<>), sourceType, () => new CoerceText())) { }
}
