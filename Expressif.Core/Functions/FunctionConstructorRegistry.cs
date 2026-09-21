using System.Reflection;
using Expressif.Discovery;

namespace Expressif.Functions;

/// <summary>
/// Associates runtime function implementations with their specialized constructors.
/// </summary>
public sealed class FunctionConstructorRegistry
{
    private readonly IReadOnlyDictionary<Type, IFunctionConstructor> constructors;

    public FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors)
    {
        var materialized = constructors.ToArray();
        foreach (var constructor in materialized)
            ValidateAssociation(constructor);
        this.constructors = Build(materialized);
    }

    public FunctionConstructorRegistry(params Assembly[] assemblies)
        : this(Discover(assemblies)) { }

    public FunctionConstructorRegistry(ITypeSource source)
        : this(Discover(source.GetTypes())) { }

    public bool TryGet(Type functionType, out IFunctionConstructor constructor)
        => constructors.TryGetValue(functionType, out constructor!);

    private static IReadOnlyDictionary<Type, IFunctionConstructor> Build(
        IEnumerable<IFunctionConstructor> constructors)
    {
        var registry = new Dictionary<Type, IFunctionConstructor>();
        foreach (var constructor in constructors)
        {
            foreach (var target in GetTargets(constructor))
            {
                if (!registry.TryAdd(target, constructor))
                {
                    throw new InvalidOperationException(
                        $"A function constructor is already registered for '{target.FullName}'.");
                }
            }
        }
        return registry;
    }

    private static void ValidateAssociation(IFunctionConstructor constructor)
    {
        if (GetTargets(constructor).Length == 0)
        {
            throw new InvalidOperationException(
                $"Function constructor '{constructor.GetType().FullName}' does not declare an IFunctionConstructor<TFunction> association.");
        }
    }

    private static Type[] GetTargets(IFunctionConstructor constructor)
        => constructor.GetType().GetInterfaces()
            .Where(candidate => candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(IFunctionConstructor<>))
            .Select(candidate => candidate.GetGenericArguments()[0])
            .ToArray();

    private static IEnumerable<IFunctionConstructor> Discover(IEnumerable<Assembly> assemblies)
        => Discover(assemblies.Distinct().SelectMany(assembly => assembly.GetTypes()));

    private static IEnumerable<IFunctionConstructor> Discover(IEnumerable<Type> types)
        => types
            .Where(type => !type.IsAbstract
                && !type.IsInterface
                && typeof(IFunctionConstructor).IsAssignableFrom(type))
            .Select(type => Activator.CreateInstance(type, nonPublic: true) as IFunctionConstructor
                ?? throw new InvalidOperationException(
                    $"Function constructor '{type.FullName}' must have a parameterless constructor."));
}
