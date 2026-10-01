using System.Reflection;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Functions.Accumulation;

internal sealed class AccumulatorRegistry : IImplementationRegistry
{
    private readonly ImplementationRegistry implementations;
    private readonly IReadOnlyDictionary<string, string> canonicalNames;

    public AccumulatorRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public AccumulatorRegistry(ITypeSource source)
        : this(Discover(source).ToArray()) { }

    private AccumulatorRegistry(AccumulatorRegistration[] accumulators)
    {
        implementations = new ImplementationRegistry(accumulators.SelectMany(accumulator => accumulator.Names
            .Select(name => new ImplementationRegistration(accumulator.Namespace, name, accumulator.Type))));
        canonicalNames = accumulators.SelectMany(accumulator => accumulator.Names.Select(alias =>
                new KeyValuePair<string, string>(ImplementationRegistry.NormalizeName(alias), accumulator.CanonicalName)))
            .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single().Value, StringComparer.OrdinalIgnoreCase);
    }

    public Type Resolve(string name) => implementations.Resolve(name);

    public bool TryResolve(string name, out Type implementationType)
        => implementations.TryResolve(name, out implementationType);

    public Type Resolve(OperatorIdentity identity) => implementations.Resolve(identity);

    public bool TryResolve(OperatorIdentity identity, out Type implementationType)
        => implementations.TryResolve(identity, out implementationType);

    public string ResolveCanonicalName(string name)
        => canonicalNames.TryGetValue(ImplementationRegistry.NormalizeName(name), out var canonicalName)
            ? canonicalName
            : ImplementationRegistry.NormalizeName(name);

    public IIncrementalAggregation Create(string name)
    {
        var implementationType = Resolve(name);
        return Activator.CreateInstance(implementationType) as IIncrementalAggregation
            ?? throw new InvalidOperationException(
                $"Accumulator '{implementationType.FullName}' must have a parameterless constructor.");
    }

    private static IEnumerable<AccumulatorRegistration> Discover(ITypeSource source)
        => source.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract
                && typeof(IIncrementalAggregation).IsAssignableFrom(type))
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<FunctionAttribute>(true)))
            .Where(candidate => candidate.Attribute is not null)
            .Select(candidate =>
            {
                var canonical = candidate.Attribute!.Name ?? candidate.Type.Name.ToKebabCase();
                var @namespace = OperatorIdentity.NamespaceFromType(candidate.Type);
                var names = candidate.Attribute.Aliases.Prepend(canonical)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                return new AccumulatorRegistration(@namespace, canonical, names, candidate.Type);
            });

    private sealed record AccumulatorRegistration(string Namespace, string CanonicalName, string[] Names, Type Type);
}
