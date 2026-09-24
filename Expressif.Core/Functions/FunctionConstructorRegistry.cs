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

    public FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors)
        : this(constructors, []) { }

    private FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors, IEnumerable<Type> types)
    {
        var materialized = constructors.ToArray();
        foreach (var constructor in materialized)
            ValidateAssociation(constructor);
        this.constructors = Build(materialized);
        annotated = DiscoverAnnotated(types);
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
            ParameterInfo? unsupported = null;
            foreach (var target in targets)
            {
                foreach (var parameter in target.GetParameters())
                {
                    var attribute = parameter.GetCustomAttribute<ArgumentEvaluationAttribute>();
                    if (attribute is null)
                        throw InvalidMetadata(type, parameter, "every constructor parameter must declare an evaluation mode");
                    var validShape = attribute.Mode switch
                    {
                        ArgumentEvaluationMode.Ambient => parameter.ParameterType.IsGenericType
                            && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<>),
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
                    if (attribute.Mode != ArgumentEvaluationMode.Incoming)
                        unsupported ??= parameter;
                }
            }
            if (unsupported is not null)
                throw InvalidMetadata(type, unsupported, "this evaluation mode is not supported by generic construction yet");
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
