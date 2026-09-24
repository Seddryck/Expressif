using System.Reflection;
using Expressif.Discovery;

namespace Expressif.Functions;

/// <summary>
/// Associates runtime function implementations with their specialized constructors.
/// </summary>
internal sealed class FunctionConstructorRegistry
{
    private readonly IReadOnlyDictionary<Type, IFunctionConstructor> constructors;
    private readonly IReadOnlyDictionary<Type, ConstructorInfo[]> annotated;
    private readonly IReadOnlyDictionary<Type, ConstructorInfo> spreadPacked;

    public FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors)
        : this(constructors, []) { }

    private FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors, IEnumerable<Type> types)
    {
        var materialized = constructors.ToArray();
        foreach (var constructor in materialized)
            ValidateAssociation(constructor);
        this.constructors = Build(materialized);
        annotated = DiscoverAnnotated(types);
        spreadPacked = DiscoverSpreadPacked(types);
    }

    public FunctionConstructorRegistry(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies)) { }

    public FunctionConstructorRegistry(ITypeSource source)
        : this(source.GetTypes().ToArray()) { }

    private FunctionConstructorRegistry(Type[] types)
        : this(Discover(types), types) { }

    public bool TryGet(Type functionType, out IFunctionConstructor constructor)
        => constructors.TryGetValue(functionType, out constructor!);

    public bool TryGetAnnotated(Type functionType, out ConstructorInfo[] targets)
        => annotated.TryGetValue(functionType, out targets!);

    public bool TryGetSpreadPacked(Type functionType, out ConstructorInfo target)
    {
        if (spreadPacked.TryGetValue(functionType, out target!))
            return true;
        return DiscoverSpreadPacked([functionType]).TryGetValue(functionType, out target!);
    }

    private static IReadOnlyDictionary<Type, ConstructorInfo> DiscoverSpreadPacked(IEnumerable<Type> types)
    {
        var result = new Dictionary<Type, ConstructorInfo>();
        foreach (var type in types.Distinct().Where(type => type.IsClass && !type.IsAbstract
            && typeof(IFunction).IsAssignableFrom(type)))
        {
            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                var packed = parameters.Select(parameter => (Parameter: parameter,
                    Attribute: parameter.GetCustomAttribute<ArgumentPackingAttribute>()))
                    .Where(item => item.Attribute is not null).ToArray();
                foreach (var (parameter, attribute) in packed)
                {
                    if (!Enum.IsDefined(attribute!.Mode))
                        throw InvalidPackingMetadata(type, parameter, "unknown packing mode");
                    if (attribute.AllowSpread && attribute.Mode != ArgumentPackingMode.Variadic)
                        throw InvalidPackingMetadata(type, parameter, "spread requires variadic packing");
                }
                var variadic = packed.Where(item => item.Attribute!.Mode == ArgumentPackingMode.Variadic).ToArray();
                if (variadic.Length > 1)
                    throw InvalidPackingMetadata(type, variadic[1].Parameter, "only one positional variadic parameter is permitted per constructor");
                foreach (var (parameter, attribute) in variadic)
                {
                    if (parameter.ParameterType != typeof(Func<object?, object?[]>))
                        throw InvalidPackingMetadata(type, parameter, "variadic packing requires Func<object?, object?[]> delegate");
                    if (parameters.Length != 1)
                        throw InvalidPackingMetadata(type, parameter, "variadic packing requires a single-parameter constructor");
                    if (attribute!.AllowSpread && !result.TryAdd(type, constructor))
                        throw InvalidPackingMetadata(type, parameter, "ambiguous spread-aware constructors");
                }
            }
        }
        return result;
    }

    private static InvalidOperationException InvalidPackingMetadata(Type type, ParameterInfo parameter, string reason)
        => new($"Invalid argument packing metadata on '{type.FullName}.{parameter.Name}': {reason}.");

    private static IReadOnlyDictionary<Type, ConstructorInfo[]> DiscoverAnnotated(IEnumerable<Type> types)
    {
        var result = new Dictionary<Type, ConstructorInfo[]>();
        var nullability = new NullabilityInfoContext();
        foreach (var type in types.Distinct().Where(type => type.IsClass && !type.IsAbstract
            && typeof(IFunction).IsAssignableFrom(type)))
        {
            var targets = type.GetConstructors();
            if (!targets.SelectMany(target => target.GetParameters())
                .Any(parameter => parameter.IsDefined(typeof(ArgumentEvaluationAttribute), false)))
                continue;

            var modes = new Dictionary<string, ArgumentEvaluationMode>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in targets)
            {
                foreach (var parameter in target.GetParameters())
                {
                    var attribute = parameter.GetCustomAttribute<ArgumentEvaluationAttribute>();
                    if (attribute is null)
                        throw InvalidMetadata(type, parameter, "every constructor parameter must declare an evaluation mode");
                    if (!Enum.IsDefined(attribute.Mode))
                        throw InvalidMetadata(type, parameter, $"unknown evaluation mode '{attribute.Mode}'");
                    var validShape = attribute.Mode switch
                    {
                        ArgumentEvaluationMode.Ambient => parameter.ParameterType.IsGenericType
                            && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<>)
                            && parameter.ParameterType.IsAssignableFrom(typeof(Func<object?>)),
                        ArgumentEvaluationMode.Incoming or ArgumentEvaluationMode.Nested =>
                            parameter.ParameterType.IsGenericType
                            && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                            && parameter.ParameterType.IsAssignableFrom(typeof(Func<object?, object?>)),
                        _ => false,
                    };
                    if (!validShape)
                    {
                        throw InvalidMetadata(type, parameter,
                            $"{attribute.Mode} requires a compatible {(attribute.Mode == ArgumentEvaluationMode.Ambient ? "zero-input" : "one-input")} delegate");
                    }
                    if (nullability.Create(parameter).ReadState == NullabilityState.Nullable && !parameter.IsOptional)
                        throw InvalidMetadata(type, parameter, "nullable delegates must be optional");

                    var name = parameter.Name!.ToKebabCase();
                    if (modes.TryGetValue(name, out var previous) && previous != attribute.Mode)
                        throw InvalidMetadata(type, parameter, "equivalent overload parameters must use the same evaluation mode");
                    modes[name] = attribute.Mode;
                }
            }
            result.Add(type, targets);
        }
        return result;
    }

    private static InvalidOperationException InvalidMetadata(Type type, ParameterInfo parameter, string reason)
        => new($"Invalid argument evaluation metadata on '{type.FullName}.{parameter.Name}': {reason}.");

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
