using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Expressif.Discovery;

namespace Expressif.Functions.Coercions;

public sealed class CoercionRegistry : ICoercionRegistry
{
    public IReadOnlyList<ICoercionDescriptor> Descriptors { get; }

    public CoercionRegistry(IEnumerable<ICoercionDescriptor> descriptors)
        => Descriptors = descriptors.ToArray();

    public CoercionRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public CoercionRegistry(ITypesProbe probe)
        : this(Discover(probe)) { }

    public bool TryResolve(
        Type sourceType,
        Type targetType,
        [NotNullWhen(true)] out string? functionName)
    {
        var descriptor = Descriptors.SingleOrDefault(candidate => candidate.Supports(sourceType, targetType));
        functionName = descriptor?.Name;
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

    public IEnumerable<CoercionInfo> Describe()
        => Descriptors.SelectMany(descriptor => descriptor.SourceTypes.Select(sourceType =>
        {
            var function = descriptor.Create(sourceType);
            return new CoercionInfo(
                descriptor.Name,
                sourceType,
                descriptor.TargetType,
                function.GetType());
        }));

    private static IEnumerable<ICoercionDescriptor> Discover(ITypesProbe probe)
        => probe.Locate()
            .Where(type => typeof(ICoercionDescriptor).IsAssignableFrom(type))
            .Select(type => Activator.CreateInstance(type, nonPublic: true) as ICoercionDescriptor
                ?? throw new InvalidOperationException(
                    $"Coercion descriptor '{type.FullName}' must have a parameterless constructor."));
}
