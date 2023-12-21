using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Predicates.Introspection;

namespace Expressif.Predicates
{
    public class PredicateTypeMapper : BaseTypeMapper
    {
        protected override IDictionary<string, Type> Initialize()
        {
            var introspector = new PredicateIntrospector();
            var infos = introspector.Locate();
            var mapping = new Dictionary<string, Type>();
            foreach (var info in infos)
            {
                mapping.Add(info.Name, info.ImplementationType);
                foreach (var alias in info.Aliases)
                    if (mapping.TryGetValue(alias, out var existing))
                        throw new InvalidOperationException($"The predicate name '{alias}' has already been added for the implementation '{existing.FullName}'. You cannot add a second time this alias for the implementation '{info.ImplementationType.FullName}'");
                    else
                        mapping.Add(alias, info.ImplementationType);
            }
            return mapping;
        }
    }
}
