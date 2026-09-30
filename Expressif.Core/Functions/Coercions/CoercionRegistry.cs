using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Expressif.Discovery;

namespace Expressif.Functions.Coercions;

public sealed class CoercionRegistry : ICoercionRegistry
{
    private IReadOnlyList<ICoercionDescriptor> Descriptors { get; }

    public CoercionRegistry(IEnumerable<ICoercionDescriptor> descriptors)
        => Descriptors = descriptors.ToArray();

    public CoercionRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public CoercionRegistry(ITypeSource source)
        : this(CoercionDescriptorDiscovery.Discover(source)) { }

    public bool TryResolve(
        Type sourceType,
        Type targetType,
        [NotNullWhen(true)] out string? functionName)
    {
        var descriptor = Descriptors.SingleOrDefault(candidate => candidate.Supports(sourceType, targetType));
        functionName = descriptor?.Name;
        return descriptor is not null;
    }

    public bool TryResolve(
        string functionName,
        [NotNullWhen(true)] out Type? targetType)
    {
        var descriptor = Descriptors.SingleOrDefault(candidate =>
            candidate.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase));
        targetType = descriptor?.TargetType;
        return descriptor is not null;
    }

    public bool TryCreate(Type sourceType, Type targetType, out IFunction coercion)
    {
        var descriptor = Descriptors.SingleOrDefault(x => x.Supports(sourceType, targetType));
        if (descriptor is null)
        {
            coercion = null!;
            return false;
        }

        coercion = descriptor.Create(sourceType);
        return true;
    }
}
