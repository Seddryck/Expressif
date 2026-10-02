using System.Reflection;
using Expressif.Functions;

namespace Expressif.Bindings;

/// <summary>
/// Associates runtime function implementations with their specialized syntax binders.
/// </summary>
public sealed class FunctionBinderRegistry
{
    private readonly IReadOnlyDictionary<Type, IFunctionBinder> binders;

    public FunctionBinderRegistry(IEnumerable<IFunctionBinder> binders)
        => this.binders = Build(binders);

    public FunctionBinderRegistry(params Assembly[] assemblies)
        : this(Discover(assemblies)) { }

    public bool TryGet(Type functionType, out IFunctionBinder binder)
        => binders.TryGetValue(functionType, out binder!);

    private static IReadOnlyDictionary<Type, IFunctionBinder> Build(IEnumerable<IFunctionBinder> binders)
    {
        var registry = new Dictionary<Type, IFunctionBinder>();
        foreach (var binder in binders)
        {
            var targets = binder.GetType().GetInterfaces()
                .Where(candidate => candidate.IsGenericType
                    && candidate.GetGenericTypeDefinition() == typeof(IFunctionBinder<>))
                .Select(candidate => candidate.GetGenericArguments()[0])
                .ToArray();
            if (targets.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Function binder '{binder.GetType().FullName}' does not declare an IFunctionBinder<TFunction> association.");
            }

            foreach (var target in targets)
            {
                if (!registry.TryAdd(target, binder))
                {
                    throw new InvalidOperationException(
                        $"A function binder is already registered for '{target.FullName}'.");
                }
            }
        }
        return registry;
    }

    private static IEnumerable<IFunctionBinder> Discover(IEnumerable<Assembly> assemblies)
        => assemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract
                && !type.IsInterface
                && typeof(IFunctionBinder).IsAssignableFrom(type))
            .Select(type => Activator.CreateInstance(type, nonPublic: true) as IFunctionBinder
                ?? throw new InvalidOperationException($"Function binder '{type.FullName}' must have a parameterless constructor."));
}
