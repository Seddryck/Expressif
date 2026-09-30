using Expressif.Library.Special;

namespace Expressif.Library.Coercions;

internal sealed class CoerceTimeDescriptor : CoercionDescriptor
{
    public CoerceTimeDescriptor()
        : base(
            "coerce-time",
            typeof(TimeOnly?),
            [typeof(TimeOnly), typeof(DateOnly), typeof(DateTime), typeof(string)],
            _ => typeof(CoerceTime),
            _ => new CoerceTime()) { }
}
