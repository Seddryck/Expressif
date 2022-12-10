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
                yield return new PredicateInfo(
                        predicate.Attribute.AppendIs
                        ? $"{predicate.Type.Namespace!.ToToken('.').Last().ToKebabCase()}-is-{predicate.Type.Name.ToKebabCase()}"
                        : $"{predicate.Type.Namespace!.ToToken('.').Last().ToKebabCase()}-{predicate.Type.Name.ToKebabCase()}"
                        , predicate.Attribute.Aliases.AsQueryable()
                            .Prepend(predicate.Type.Name.ToKebabCase()).ToArray()
                        , predicate.Type.Namespace!.ToToken('.').Last()
                        , predicate.Type
                    );
            }
        }

    }
}
