using System.Reflection;

namespace Expressif.Discovery;

public class AssemblyTypesProbe : ITypesProbe
{
    public Assembly[] Assemblies { get; } = [typeof(IExpression).Assembly];

    public AssemblyTypesProbe()
    { }

    public AssemblyTypesProbe(Assembly[] assemblies)
        => Assemblies = assemblies;

    public virtual IEnumerable<Type> Locate()
        => Assemblies.Aggregate(
            System.Array.Empty<Type>(),
            (types, assembly) => types.Concat(
                assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract)).ToArray());
}

public interface ITypesProbe
{
    IEnumerable<Type> Locate();
}
