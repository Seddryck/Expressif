using Expressif.Library.Special;
using Expressif.Values;

namespace Expressif.Library.Coercions;

internal sealed class CoerceOrderingDescriptor : CoercionDescriptor
{
    public CoerceOrderingDescriptor()
        : base(
            "coerce-ordering",
            typeof(OrderingValue),
            CoercionDescriptorSupport.NumericSourceTypes,
            sourceType => (IFunction)Activator.CreateInstance(
                typeof(CoerceOrdering<>).MakeGenericType(sourceType))!) { }
}
