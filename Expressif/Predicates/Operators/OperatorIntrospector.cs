using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Introspection;

namespace Expressif.Predicates.Operators;
internal class OperatorIntrospector : BaseIntrospector
{
    public OperatorIntrospector()
            : this(new AssemblyTypesProbe()) { }
    public OperatorIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
    public OperatorIntrospector(ITypesProbe probe)
        : base(probe) { }

    public IEnumerable<OperatorInfo> Locate()
    {
        var operators = LocateAttribute<OperatorAttribute>();

        foreach (var @operator in operators)
        {
            yield return new OperatorInfo(
                    @operator.Type.Name.ToKebabCase()
                    , @operator.Type.IsPublic
                    , @operator.Attribute.Aliases
                    , @operator.Type
                    , @operator.Type.GetSummary()
                );
        }
    }
}
