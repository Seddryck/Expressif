using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Predicates.Operators;
internal class OperatorTypeMapper<T> : BaseTypeMapper
{
    protected override IDictionary<string, Type> Initialize()
    {
        var introspector = new OperatorIntrospector();
        var infos = introspector.Locate().Where(x => x.ImplementationType.IsAssignableTo(typeof(T)));
        var mapping = new Dictionary<string, Type>();
        foreach (var info in infos)
        {
            foreach (var alias in new[] { info.Name.Replace("-operator", "") }.Union(info.Aliases))
                if (mapping.TryGetValue(alias, out var existing))
                    throw new InvalidOperationException($"The operator name '{alias}' has already been added for the implementation '{existing.FullName}'. You cannot add a second times this alias for the implementation '{info.ImplementationType.FullName}'");
                else
                    mapping.Add(alias, info.ImplementationType);
        }
        return mapping;
    }
}
