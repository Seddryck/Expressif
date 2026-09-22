using Expressif.Library.Special;
using Expressif.Values;

namespace Expressif.Library.Coercions;

internal sealed class CoerceDateDescriptor : CoercionDescriptor
{
    public CoerceDateDescriptor()
        : base(
            "coerce-date",
            typeof(DateOnly?),
            [typeof(DateOnly), typeof(DateTime), typeof(YearMonth), typeof(string)],
            _ => typeof(CoerceDate),
            _ => new CoerceDate()) { }
}
