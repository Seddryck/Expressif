using System.Reflection;
using Expressif.Functions.Accumulation;
using Expressif.Discovery;

namespace Expressif.Functions;

public sealed class FunctionRegistry : ImplementationRegistry
{
    public FunctionRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public FunctionRegistry(ITypeSource source)
        : base(Discover(source)) { }

    private static IEnumerable<ImplementationRegistration> Discover(ITypeSource source)
        => source.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract
                && !typeof(IAccumulator).IsAssignableFrom(type))
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<FunctionAttribute>(true)))
            .Where(candidate => candidate.Attribute is not null)
            .SelectMany(candidate => Names(candidate.Type, candidate.Attribute!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => new ImplementationRegistration(name, candidate.Type)));

    private static IEnumerable<string> Names(Type type, FunctionAttribute attribute)
    {
        var name = attribute.Name ?? type.Name.ToKebabCase();
        yield return name;
        foreach (var alias in attribute.Aliases)
            yield return alias;
        if (attribute.Prefix is null)
        {
            var prefix = type.GetCustomAttribute<ScopeAttribute>(true)?.Name.Split('/')[0]
                ?? type.Namespace!.Split('.').Last().ToKebabCase();
            yield return $"{prefix}-to-{type.Name.ToKebabCase()}";
        }
        else if (attribute.Prefix.Length > 0)
        {
            yield return $"{attribute.Prefix}-to-{type.Name.ToKebabCase()}";
        }
    }
}
