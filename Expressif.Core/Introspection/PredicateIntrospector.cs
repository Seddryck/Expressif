using System.Reflection;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif.Introspection;

/// <summary>Describes predicates discovered from extension assemblies or a custom type source.</summary>
public sealed class PredicateIntrospector
{
    private readonly BaseIntrospector scanner;
    private readonly IntrospectionOptions options;
    private readonly ExpressifTypeMapper typeMapper;

    public PredicateIntrospector(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(RequireAssemblies(assemblies)), IntrospectionOptions.Default) { }

    public PredicateIntrospector(ITypeSource source)
        : this(source, IntrospectionOptions.Default) { }

    internal PredicateIntrospector(ITypeSource source, IntrospectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.options = options;
        scanner = new BaseIntrospector(source);
        typeMapper = new ExpressifTypeMapper(options);
    }

    public IReadOnlyList<PredicateInfo> Describe()
        => DescribePredicates().ToList().AsReadOnly();

    private IEnumerable<PredicateInfo> DescribePredicates()
    {
        foreach (var predicate in scanner.LocateAttribute<PredicateAttribute>())
        {
            var scope = predicate.Type.GetCustomAttribute<ScopeAttribute>(true)?.Name
                ?? predicate.Type.Namespace!.ToToken('.').Last().ToKebabCase();
            var prefix = predicate.Attribute.Prefix != null && string.IsNullOrEmpty(predicate.Attribute.Prefix)
                ? string.Empty
                : string.IsNullOrEmpty(predicate.Attribute.Prefix)
                    ? scope.Split('/')[0]
                    : predicate.Attribute.Prefix;
            var typeName = predicate.Type.Name.ToKebabCase();
            var suffix = predicate.Attribute.AppendIs ? "is" : string.Empty;
            var canonicalName = predicate.Attribute.Name
                ?? string.Join('-', new[] { suffix, typeName }.Where(value => !string.IsNullOrEmpty(value)));
            var prefixedName = string.Join('-', new[] { prefix, suffix, typeName }
                .Where(value => !string.IsNullOrEmpty(value)));
            var aliases = predicate.Attribute.Aliases
                .Append(typeName)
                .Append(prefixedName)
                .Where(alias => !string.Equals(alias, canonicalName, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            yield return new PredicateInfo(new PredicateInfoDefinition
            {
                Name = canonicalName,
                IsPublic = predicate.Type.IsPublic,
                Aliases = aliases,
                Scope = scope,
                ImplementationType = predicate.Type,
                Summary = predicate.Type.GetSummary(),
                Parameters = BaseIntrospector.BuildParameters(predicate.Type.GetInfoConstructors(typeMapper, options)),
                Signatures = options.TupleBindingSignatures(predicate.Type).Select(signature => signature.ToInfo()),
            });
        }
    }

    private static Assembly[] RequireAssemblies(Assembly[] assemblies)
        => assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
}
