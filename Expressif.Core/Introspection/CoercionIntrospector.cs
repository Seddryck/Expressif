using System.Reflection;
using Expressif.Discovery;
using Expressif.Functions.Coercions;

namespace Expressif.Introspection;

/// <summary>Describes coercions discovered from extension assemblies or a custom type source.</summary>
public sealed class CoercionIntrospector
{
    private readonly IReadOnlyList<CoercionInfo> descriptions;

    public CoercionIntrospector(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(RequireAssemblies(assemblies))) { }

    public CoercionIntrospector(ITypeSource source)
        : this(CoercionDescriptorDiscovery.Discover(source)) { }

    internal CoercionIntrospector(IEnumerable<ICoercionDescriptor> descriptors)
    {
        descriptions = descriptors
            .SelectMany(descriptor => descriptor.SourceTypes
                .OrderBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal)
                .Select(sourceType => new CoercionInfo(
                    descriptor.Name,
                    sourceType,
                    descriptor.TargetType,
                    descriptor.GetImplementationType(sourceType))))
            .OrderBy(info => info.Name, StringComparer.Ordinal)
            .ThenBy(info => info.SourceType.AssemblyQualifiedName, StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<CoercionInfo> Describe() => descriptions;

    private static Assembly[] RequireAssemblies(Assembly[] assemblies)
        => assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
}
