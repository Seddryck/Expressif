using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Discovery;

namespace Expressif.Library.Operators;

internal class OperatorIntrospector
{
    private readonly BaseIntrospector scanner;

    public OperatorIntrospector()
            : this(new AssemblyTypeSource(typeof(OperatorIntrospector).Assembly)) { }
    public OperatorIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Distinct().ToArray())) { }
    public OperatorIntrospector(ITypeSource source)
        => scanner = new BaseIntrospector(source);

    public IEnumerable<OperatorInfo> Locate()
        => Locate(true);

    public IEnumerable<OperatorInfo> Describe()
        => Locate(false);

    protected IEnumerable<OperatorInfo> Locate(bool fast = true)
    {
        var operators = scanner.LocateAttribute<OperatorAttribute>();

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
