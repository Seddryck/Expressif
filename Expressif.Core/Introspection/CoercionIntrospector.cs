using Expressif.Functions.Coercions;

namespace Expressif.Introspection;

public sealed class CoercionIntrospector
{
    private ICoercionRegistry Registry { get; }

    public CoercionIntrospector(ICoercionRegistry registry)
        => Registry = registry;

    public IEnumerable<CoercionInfo> Locate()
        => Registry.Describe();
}
