using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Discovery;

namespace Expressif.Library.Operators;

internal class OperatorIntrospector : BaseIntrospector
{
    public OperatorIntrospector()
            : this(new AssemblyTypesProbe([typeof(OperatorIntrospector).Assembly])) { }
    public OperatorIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
    public OperatorIntrospector(ITypesProbe probe)
        : base(probe) { }

    public IEnumerable<OperatorInfo> Locate()
        => Locate(true);

    public IEnumerable<OperatorInfo> Describe()
        => Locate(false);

    protected IEnumerable<OperatorInfo> Locate(bool fast = true)
    {
        var operators = LocateAttribute<OperatorAttribute>();

        foreach (var @operator in operators)
        {
            yield return new OperatorInfo(
                    @operator.Type.Name.ToKebabCase()
                    , @operator.Type.IsPublic
                    , @operator.Attribute.Aliases
                    , @operator.Type
                    , fast ? "" : @operator.Type.GetSummary()
                );
        }
    }
}
