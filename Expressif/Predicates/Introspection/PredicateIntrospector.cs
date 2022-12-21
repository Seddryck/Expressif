using Expressif.Functions.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Introspection
{
    public class PredicateIntrospector : BaseIntrospector
    {
        public PredicateIntrospector()
            : this(new AssemblyTypesProbe()) { }
        public PredicateIntrospector(Assembly[] assemblies)
            : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
        public PredicateIntrospector(ITypesProbe probe)
            : base(probe) { }

        public IEnumerable<PredicateInfo> Locate()
            => Locate<PredicateAttribute>();

        protected IEnumerable<PredicateInfo> Locate<T>() where T : PredicateAttribute
        {
            var predicates = LocateAttribute<PredicateAttribute>();

            foreach (var predicate in predicates)
            {
                var prefix = predicate.Attribute.Prefix != null && string.IsNullOrEmpty(predicate.Attribute.Prefix)
                                        ? string.Empty
                                        : string.IsNullOrEmpty(predicate.Attribute.Prefix)
                                            ? predicate.Type.Namespace!.Split('.').Last().ToKebabCase()
                                            : predicate.Attribute.Prefix;

                var suffix = predicate.Attribute.AppendIs ? $"is" : string.Empty;

                var array = new[] { prefix, suffix, predicate.Type.Name.ToKebabCase() }.Where(x => !string.IsNullOrEmpty(x));

                yield return new PredicateInfo(
                        predicate.Type.Name.ToKebabCase()
                        , predicate.Attribute.Aliases.AsQueryable().Prepend(string.Join('-', array)).ToArray()
                        , predicate.Type.Namespace!.ToToken('.').Last()
                        , predicate.Type
                        , predicate.Type.GetSummary()
                        , BuildParameters(predicate.Type.GetInfoConstructors()).ToArray()
                    );
            }
        }
    }
}
