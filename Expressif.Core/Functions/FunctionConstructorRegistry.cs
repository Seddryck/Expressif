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
    private readonly IReadOnlyDictionary<Type, ConstructorInfo> variadicPacked;
    private readonly IReadOnlyDictionary<Type, ConstructorInfo[]> roleAnnotated;
    private readonly IReadOnlyDictionary<Type, ConstructorInfo[]> shapeAnnotated;

    public FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors)
        : this(constructors, []) { }

    private FunctionConstructorRegistry(IEnumerable<IFunctionConstructor> constructors, IEnumerable<Type> types)
    {
        var materialized = constructors.ToArray();
        foreach (var constructor in materialized)
            ValidateAssociation(constructor);
        this.constructors = Build(materialized);
        annotated = DiscoverAnnotated(types);
        variadicPacked = DiscoverVariadicPacked(types);
        roleAnnotated = DiscoverRoles(types);
        shapeAnnotated = DiscoverShapes(types);
        foreach (var type in types.Where(type => type.IsClass && !type.IsAbstract
            && typeof(IFunction).IsAssignableFrom(type)))
        {
            Expressif.Bindings.ParameterArgumentBinder.ValidateLayoutMetadata(type);
            ValidateOmissionsAndLifetimes(type);
        }
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

    public bool TryGetVariadicPacked(Type functionType, out ConstructorInfo target)
    {
        if (variadicPacked.TryGetValue(functionType, out target!))
            return true;
        return DiscoverVariadicPacked([functionType]).TryGetValue(functionType, out target!);
    }

    public bool TryGetRoleAnnotated(Type functionType, out ConstructorInfo[] targets)
    {
        if (roleAnnotated.TryGetValue(functionType, out targets!))
            return true;
        return DiscoverRoles([functionType]).TryGetValue(functionType, out targets!);
    }

    public bool TryGetShapeAnnotated(Type functionType, out ConstructorInfo[] targets)
    {
        if (shapeAnnotated.TryGetValue(functionType, out targets!))
            return true;
        return DiscoverShapes([functionType]).TryGetValue(functionType, out targets!);
    }

    private static IReadOnlyDictionary<Type, ConstructorInfo[]> DiscoverShapes(IEnumerable<Type> types)
    {
        var result = new Dictionary<Type, ConstructorInfo[]>();
        foreach (var type in types.Distinct().Where(type => type.IsClass && !type.IsAbstract
            && typeof(IFunction).IsAssignableFrom(type)))
        {
            var targets = type.GetConstructors().Where(constructor => constructor.GetParameters()
                .Any(parameter => parameter.IsDefined(typeof(AcceptedExpressionShapeAttribute), false))).ToArray();
            if (targets.Length == 0)
                continue;
            var shapes = new Dictionary<string, AcceptedExpressionShape>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in targets)
            {
                foreach (var parameter in target.GetParameters())
                {
                    var attribute = parameter.GetCustomAttribute<AcceptedExpressionShapeAttribute>();
                    if (attribute is null)
                        continue;
                    if (!Enum.IsDefined(attribute.Shape))
                        throw InvalidShapeMetadata(type, parameter, "unknown expression shape");
                    var expectedType = attribute.Shape switch
                    {
                        AcceptedExpressionShape.DirectFieldSelector => typeof(Expressif.Bindings.NamedFieldSelector),
                        AcceptedExpressionShape.OpenExpression => typeof(Func<IFunction>),
                        AcceptedExpressionShape.CallableReference => parameter.ParameterType,
                        _ => throw InvalidShapeMetadata(type, parameter, "unknown expression shape"),
                    };
                    if (parameter.ParameterType != expectedType)
                    {
                        throw InvalidShapeMetadata(type, parameter,
                            $"{attribute.Shape} requires a {expectedType.Name} runtime parameter");
                    }
                    if (attribute.Shape == AcceptedExpressionShape.CallableReference
                        && (!parameter.ParameterType.IsGenericType
                            || parameter.ParameterType.GetGenericTypeDefinition() != typeof(Func<>)))
                    {
                        throw InvalidShapeMetadata(type, parameter,
                            "CallableReference requires a zero-input provider delegate");
                    }
                    var name = parameter.Name!.ToKebabCase();
                    if (shapes.TryGetValue(name, out var previous) && previous != attribute.Shape)
                        throw InvalidShapeMetadata(type, parameter, "equivalent overload parameters must use the same expression shape");
                    shapes[name] = attribute.Shape;
                }
            }
            result.Add(type, targets);
        }
        return result;
    }

    private static InvalidOperationException InvalidShapeMetadata(Type type, ParameterInfo parameter, string reason)
        => new($"Invalid expression shape metadata on '{type.FullName}.{parameter.Name}': {reason}.");

    private static void ValidateOmissionsAndLifetimes(Type type)
    {
        var nullability = new NullabilityInfoContext();
        var omissions = new Dictionary<string, ArgumentOmissionMode>(StringComparer.OrdinalIgnoreCase);
        foreach (var parameter in type.GetConstructors().SelectMany(constructor => constructor.GetParameters()))
        {
            var omission = parameter.GetCustomAttribute<ArgumentOmissionAttribute>();
            if (omission is not null)
            {
                if (!Enum.IsDefined(omission.Mode))
                    throw InvalidContract(type, parameter, "unknown omission mode");
                if (omission.Mode == ArgumentOmissionMode.EmptyVariadic
                    && parameter.GetCustomAttribute<ArgumentPackingAttribute>() is not
                        { Mode: ArgumentPackingMode.Variadic })
                {
                    throw InvalidContract(type, parameter, "empty-variadic omission requires variadic packing");
                }
                if (omission.Mode == ArgumentOmissionMode.Absent
                    && (!parameter.IsOptional || nullability.Create(parameter).ReadState != NullabilityState.Nullable))
                {
                    throw InvalidContract(type, parameter, "absent omission requires an optional nullable parameter");
                }
                var name = parameter.Name!.ToKebabCase();
                if (omissions.TryGetValue(name, out var previous) && previous != omission.Mode)
                    throw InvalidContract(type, parameter, "equivalent overload parameters must use the same omission mode");
                omissions[name] = omission.Mode;
            }

            var lifetime = parameter.GetCustomAttribute<ProviderLifetimeAttribute>();
            if (lifetime is null)
                continue;
            if (!Enum.IsDefined(lifetime.Lifetime))
                throw InvalidContract(type, parameter, "unknown provider lifetime");
            var role = parameter.GetCustomAttribute<ArgumentRoleAttribute>()?.Role;
            if (role is not (ArgumentRole.Predicate or ArgumentRole.Transformation or ArgumentRole.Accumulator)
                || !parameter.ParameterType.IsGenericType
                || parameter.ParameterType.GetGenericTypeDefinition() != typeof(Func<>))
            {
                throw InvalidContract(type, parameter, "provider lifetime requires a semantic-role provider delegate");
            }
            if (role == ArgumentRole.Accumulator && lifetime.Lifetime != ProviderLifetime.FreshPerRequest)
                throw InvalidContract(type, parameter, "accumulator providers must be fresh per request");
        }
    }

    private static InvalidOperationException InvalidContract(Type type, ParameterInfo parameter, string reason)
        => new($"Invalid constructor contract on '{type.FullName}.{parameter.Name}': {reason}.");

    private static IReadOnlyDictionary<Type, ConstructorInfo[]> DiscoverRoles(IEnumerable<Type> types)
    {
        var result = new Dictionary<Type, ConstructorInfo[]>();
        foreach (var type in types.Distinct().Where(type => type.IsClass && !type.IsAbstract
            && typeof(IFunction).IsAssignableFrom(type)))
        {
            var targets = type.GetConstructors().Where(constructor => constructor.GetParameters()
                .Any(parameter => parameter.IsDefined(typeof(ArgumentRoleAttribute), false))).ToArray();
            if (targets.Length == 0)
                continue;
            var roles = new Dictionary<string, ArgumentRole>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in targets)
            {
                foreach (var parameter in target.GetParameters())
                {
                    var attribute = parameter.GetCustomAttribute<ArgumentRoleAttribute>()
                        ?? throw InvalidRoleMetadata(type, parameter, "every parameter in a role-annotated constructor must declare a role");
                    if (!Enum.IsDefined(attribute.Role))
                        throw InvalidRoleMetadata(type, parameter, "unknown semantic role");
                    var expectedType = attribute.Role switch
                    {
                        ArgumentRole.Predicate => typeof(Func<Predicates.IPredicate>),
                        ArgumentRole.Transformation => typeof(Func<IFunction>),
                        ArgumentRole.Accumulator => typeof(Func<Accumulation.IAccumulator>),
                        _ => throw InvalidRoleMetadata(type, parameter, "unknown semantic role"),
                    };
                    if (parameter.ParameterType != expectedType)
                    {
                        throw InvalidRoleMetadata(type, parameter,
                            $"{attribute.Role} requires a {expectedType.Name} provider delegate");
                    }
                    var name = parameter.Name!.ToKebabCase();
                    if (roles.TryGetValue(name, out var previous) && previous != attribute.Role)
                        throw InvalidRoleMetadata(type, parameter, "equivalent overload parameters must use the same semantic role");
                    roles[name] = attribute.Role;
                }
            }
            result.Add(type, targets);
        }
        return result;
    }

    private static InvalidOperationException InvalidRoleMetadata(Type type, ParameterInfo parameter, string reason)
        => new($"Invalid argument role metadata on '{type.FullName}.{parameter.Name}': {reason}.");

    private static IReadOnlyDictionary<Type, ConstructorInfo> DiscoverVariadicPacked(IEnumerable<Type> types)
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
                    var typed = parameter.ParameterType.IsGenericType
                        && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<>)
                        && parameter.ParameterType.GetGenericArguments()[0].IsArray;
                    if (parameter.ParameterType != typeof(Func<object?, object?[]>) && (!typed || attribute!.AllowSpread))
                    {
                        throw InvalidPackingMetadata(type, parameter,
                            attribute!.AllowSpread
                                ? "spread-aware variadic packing requires Func<object?, object?[]> delegate"
                                : "variadic packing requires Func<object?, object?[]> or Func<T[]> delegate");
                    }
                    if (parameters.Length != 1)
                        throw InvalidPackingMetadata(type, parameter, "variadic packing requires a single-parameter constructor");
                    if (!result.TryAdd(type, constructor))
                        throw InvalidPackingMetadata(type, parameter, "ambiguous variadic constructors");
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
                            (parameter.ParameterType.IsGenericType
                                && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                                && parameter.ParameterType.IsAssignableFrom(typeof(Func<object?, object?>)))
                            || parameter.ParameterType == typeof(IEnumerable<Func<object?, object?>>)
                            || TryGetNamedEvaluatorConstructor(parameter.ParameterType, out _),
                        _ => false,
                    };
                    if (!validShape)
                    {
                        throw InvalidMetadata(type, parameter,
                            $"{attribute.Mode} requires a compatible {(attribute.Mode == ArgumentEvaluationMode.Ambient ? "zero-input" : "one-input")} delegate");
                    }
                    if (nullability.Create(parameter).ReadState == NullabilityState.Nullable && !parameter.IsOptional)
                        throw InvalidMetadata(type, parameter, "nullable delegates must be optional");
                    if (parameter.ParameterType == typeof(IEnumerable<Func<object?, object?>>)
                        && (target.GetParameters().Length != 1
                            || target.GetCustomAttribute<Expressif.Bindings.ArgumentLayoutAttribute>() is not
                                { Kind: Expressif.Bindings.ArgumentLayoutKind.Positional }))
                    {
                        throw InvalidMetadata(type, parameter,
                            "expression collections require a single-parameter positional argument layout");
                    }
                    if (TryGetNamedEvaluatorConstructor(parameter.ParameterType, out _)
                        && (target.GetParameters().Length != 1
                            || target.GetCustomAttribute<Expressif.Bindings.ArgumentLayoutAttribute>() is not
                                { Kind: Expressif.Bindings.ArgumentLayoutKind.Named }))
                    {
                        throw InvalidMetadata(type, parameter,
                            "named evaluator collections require a single-parameter named argument layout");
                    }

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

    internal static bool TryGetNamedEvaluatorConstructor(Type parameterType, out ConstructorInfo constructor)
    {
        constructor = null!;
        if (!parameterType.IsGenericType || parameterType.GetGenericTypeDefinition() != typeof(Func<>))
            return false;
        var resultType = parameterType.GetGenericArguments()[0];
        if (!resultType.IsArray)
            return false;
        constructor = resultType.GetElementType()!.GetConstructor(
            [typeof(string), typeof(Func<object?, object?>)])!;
        return constructor is not null;
    }

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
