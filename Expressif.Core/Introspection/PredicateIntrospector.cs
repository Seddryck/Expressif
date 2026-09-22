using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Discovery;
using Expressif.Predicates;

namespace Expressif.Introspection;

public class PredicateIntrospector : BaseIntrospector
{
    private IntrospectionOptions Options { get; }
    private ExpressifTypeMapper TypeMapper { get; }

    public PredicateIntrospector(IntrospectionOptions options, params Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray()), options) { }

    public PredicateIntrospector(ITypesProbe probe, IntrospectionOptions options)
        : base(probe)
        => (Options, TypeMapper) = (options, new ExpressifTypeMapper(options));

    public IEnumerable<PredicateInfo> Locate()
        => Locate<PredicateAttribute>(true);

    public IEnumerable<PredicateInfo> Describe()
        => Locate<PredicateAttribute>(false);

    protected IEnumerable<PredicateInfo> Locate<T>(bool fast = true)
        where T : PredicateAttribute
    {
        var predicates = LocateAttribute<PredicateAttribute>();

        foreach (var predicate in predicates)
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
                ?? string.Join('-', new[] { suffix, typeName }.Where(x => !string.IsNullOrEmpty(x)));
            var prefixedName = string.Join('-', new[] { prefix, suffix, typeName }.Where(x => !string.IsNullOrEmpty(x)));
            var aliases = predicate.Attribute.Aliases
                .Append(typeName)
                .Append(prefixedName)
                .Where(x => !string.Equals(x, canonicalName, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            yield return new PredicateInfo(
                    canonicalName
                    , predicate.Type.IsPublic
                    , aliases
                    , scope
                    , predicate.Type
                    , fast ? "" : predicate.Type.GetSummary()
                    , fast ? [] : BuildParameters(predicate.Type.GetInfoConstructors(TypeMapper, Options)).ToArray()
                    , Options.TupleBindingSignatures(predicate.Type)
                );
        }
    }
}
