using Expressif.Functions.Coercions;

namespace Expressif.Functions.Introspection;

public sealed class CoercionIntrospector
{
    private CoercionRegistry Registry { get; }

    public CoercionIntrospector()
        : this(new CoercionRegistry()) { }

    public CoercionIntrospector(CoercionRegistry registry)
        => Registry = registry;

    public IEnumerable<CoercionInfo> Locate()
        => Registry.Describe();
}
