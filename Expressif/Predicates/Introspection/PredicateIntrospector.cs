using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Introspection;

namespace Expressif.Predicates.Introspection;

public class PredicateIntrospector : BaseIntrospector
{
    public PredicateIntrospector()
        : this(new AssemblyTypesProbe()) { }
    public PredicateIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
    public PredicateIntrospector(ITypesProbe probe)
        : base(probe) { }

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
            var prefix = predicate.Attribute.Prefix != null && string.IsNullOrEmpty(predicate.Attribute.Prefix)
                                    ? string.Empty
                                    : string.IsNullOrEmpty(predicate.Attribute.Prefix)
                                        ? predicate.Type.Namespace!.Split('.').Last().ToKebabCase()
                                        : predicate.Attribute.Prefix;

            var typeName = predicate.Type.Name.ToKebabCase();
            var suffix = predicate.Attribute.AppendIs ? "is" : string.Empty;
            var canonicalName = string.Join('-', new[] { suffix, typeName }.Where(x => !string.IsNullOrEmpty(x)));
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
                    , predicate.Type.Namespace!.ToToken('.').Last()
                    , predicate.Type
                    , fast ? "" : predicate.Type.GetSummary()
                    , fast ? [] : BuildParameters(predicate.Type.GetInfoConstructors()).ToArray()
                );
        }
    }
}
