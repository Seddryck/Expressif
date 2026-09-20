using Expressif.Library.Special;
using Expressif.Values;

namespace Expressif.Library.Coercions;

internal sealed class CoerceDateTimeDescriptor : CoercionDescriptor
{
    public CoerceDateTimeDescriptor()
        : base(
            "coerce-datetime",
            typeof(DateTime?),
            [typeof(DateTime), typeof(DateOnly), typeof(YearMonth), typeof(string)],
            _ => new CoerceDateTime()) { }
}
