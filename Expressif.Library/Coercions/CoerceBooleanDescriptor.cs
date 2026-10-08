using Expressif.Library.Special;

namespace Expressif.Library.Coercions;

internal sealed class CoerceBooleanDescriptor : CoercionDescriptor
{
    public CoerceBooleanDescriptor()
        : base(
            "coerce-boolean",
            typeof(bool?),
            CoercionDescriptorSupport.NumericSourceTypes.Concat([typeof(bool), typeof(string)]),
            sourceType => CoercionDescriptorSupport.GetNumericOrFallbackType(
                typeof(CoerceBoolean<>), typeof(CoerceBoolean), sourceType),
            sourceType => CoercionDescriptorSupport.CreateNumericOrFallback(
                typeof(CoerceBoolean<>), sourceType, () => new CoerceBoolean())) { }
}
