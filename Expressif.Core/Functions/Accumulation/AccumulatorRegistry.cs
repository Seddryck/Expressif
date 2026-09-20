using System.Reflection;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Functions.Accumulation;

public sealed class AccumulatorRegistry : IImplementationRegistry
{
    private readonly ImplementationRegistry implementations;
    private readonly IReadOnlyDictionary<string, string> canonicalNames;

    public AccumulatorRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public AccumulatorRegistry(ITypesProbe probe)
        : this(Discover(probe).ToArray()) { }

    private AccumulatorRegistry(AccumulatorRegistration[] accumulators)
    {
        implementations = new ImplementationRegistry(accumulators.SelectMany(accumulator => accumulator.Names
            .Select(name => new ImplementationRegistration(name, accumulator.Type))));
        canonicalNames = accumulators.SelectMany(accumulator => accumulator.Names.Select(alias =>
                new KeyValuePair<string, string>(ImplementationRegistry.NormalizeName(alias), accumulator.CanonicalName)))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    public Type Resolve(string name) => implementations.Resolve(name);

    public bool TryResolve(string name, out Type implementationType)
        => implementations.TryResolve(name, out implementationType);

    public string ResolveCanonicalName(string name)
        => canonicalNames.TryGetValue(ImplementationRegistry.NormalizeName(name), out var canonicalName)
            ? canonicalName
            : ImplementationRegistry.NormalizeName(name);

    public IAccumulator Create(string name)
    {
        var implementationType = Resolve(name);
        return Activator.CreateInstance(implementationType) as IAccumulator
            ?? throw new InvalidOperationException(
                $"Accumulator '{implementationType.FullName}' must have a parameterless constructor.");
    }

    private static IEnumerable<AccumulatorRegistration> Discover(ITypesProbe probe)
        => probe.Locate()
            .Where(type => typeof(IAccumulator).IsAssignableFrom(type))
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<FunctionAttribute>(true)))
            .Where(candidate => candidate.Attribute is not null)
            .Select(candidate =>
            {
                var canonical = candidate.Attribute!.Name ?? candidate.Type.Name.ToKebabCase();
                var names = candidate.Attribute.Aliases.Prepend(canonical)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                return new AccumulatorRegistration(canonical, names, candidate.Type);
            });

    private sealed record AccumulatorRegistration(string CanonicalName, string[] Names, Type Type);
}
