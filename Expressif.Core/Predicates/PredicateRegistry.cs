using System.Reflection;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Predicates;

internal sealed class PredicateRegistry : ImplementationRegistry
{
    public PredicateRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public PredicateRegistry(ITypeSource source)
        : base(Discover(source)) { }

    private static IEnumerable<ImplementationRegistration> Discover(ITypeSource source)
        => source.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<PredicateAttribute>(true)))
            .Where(candidate => candidate.Attribute is not null)
            .SelectMany(candidate => Names(candidate.Type, candidate.Attribute!)
                .Select(name => new ImplementationRegistration(name, candidate.Type)));

    private static IEnumerable<string> Names(Type type, PredicateAttribute attribute)
    {
        var prefix = attribute.Prefix is not null
            ? attribute.Prefix
            : type.GetCustomAttribute<ScopeAttribute>(true)?.Name.Split('/')[0]
                ?? type.Namespace!.Split('.').Last().ToKebabCase();
        var typeName = type.Name.ToKebabCase();
        var suffix = attribute.AppendIs ? "is" : string.Empty;
        var canonical = attribute.Name
            ?? string.Join('-', new[] { suffix, typeName }.Where(value => value.Length > 0));
        yield return canonical;
        foreach (var alias in attribute.Aliases.Append(typeName)
                     .Append(string.Join('-', new[] { prefix, suffix, typeName }.Where(value => value.Length > 0)))
                     .Where(alias => !alias.Equals(canonical, StringComparison.OrdinalIgnoreCase))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            yield return alias;
        }
    }
}
