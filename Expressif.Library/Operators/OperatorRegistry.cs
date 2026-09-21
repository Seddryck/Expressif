using System.Reflection;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Library.Operators;

internal sealed class OperatorRegistry<TOperator> : ImplementationRegistry
{
    public OperatorRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies)))) { }

    public OperatorRegistry(ITypeSource source)
        : base(Discover(source)) { }

    private static IEnumerable<ImplementationRegistration> Discover(ITypeSource source)
        => new OperatorIntrospector(source).Locate()
            .Where(info => info.ImplementationType.IsAssignableTo(typeof(TOperator)))
            .SelectMany(info => info.Aliases.Prepend(info.Name.Replace("-operator", string.Empty, StringComparison.Ordinal))
                .Select(name => new ImplementationRegistration(name, info.ImplementationType)));
}
