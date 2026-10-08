using Expressif.Discovery;

namespace Expressif.Functions.Coercions;

internal static class CoercionDescriptorDiscovery
{
    public static IReadOnlyList<ICoercionDescriptor> Discover(ITypeSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract
                && typeof(ICoercionDescriptor).IsAssignableFrom(type))
            .OrderBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal)
            .Select(type => Activator.CreateInstance(type, nonPublic: true) as ICoercionDescriptor
                ?? throw new InvalidOperationException(
                    $"Coercion descriptor '{type.FullName}' must have a parameterless constructor."))
            .ToList()
            .AsReadOnly();
    }
}
