using System.Reflection;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Library.Operators;

internal sealed class OperatorRegistry<TOperator> : ImplementationRegistry
{
    public OperatorRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public OperatorRegistry(ITypesProbe probe)
        : base(Discover(probe)) { }

    private static IEnumerable<ImplementationRegistration> Discover(ITypesProbe probe)
        => new OperatorIntrospector(probe).Locate()
            .Where(info => info.ImplementationType.IsAssignableTo(typeof(TOperator)))
            .SelectMany(info => info.Aliases.Prepend(info.Name.Replace("-operator", string.Empty, StringComparison.Ordinal))
                .Select(name => new ImplementationRegistration(name, info.ImplementationType)));
}
