using Expressif.Functions.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions
{
    public class FunctionTypeMapper : BaseTypeMapper
    {
        protected override IDictionary<string, Type> Initialize()
        {
            var introspector = new FunctionIntrospector();
            var infos = introspector.Locate();
            var mapping = new Dictionary<string, Type>();
            foreach (var info in infos)
            {
                foreach (var alias in new[] { info.Name }.Union(info.Aliases))
                    if (mapping.TryGetValue(alias, out var existing))
                        throw new InvalidOperationException($"The function name '{alias}' has already been added for the implementation '{existing.FullName}'. You cannot add a second times this alias for the implementation '{info.ImplementationType.FullName}'");
                    else
                        mapping.Add(alias, info.ImplementationType);
            }
            return mapping;
        }
    }
}
