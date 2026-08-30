using Expressif.Functions.Introspection;
using System.Reflection;

namespace Expressif.Types;

public sealed record TypeLiteralMetadata(string? Syntax, string[] Examples);

public sealed record TypeDescriptor(
    string Name,
    string Summary,
    string? Parent,
    TypeLiteralMetadata? Literal,
    IReadOnlyDictionary<string, string> Bindings);

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ExpressifTypeAttribute : Attribute
{
    public string? Name { get; init; }
    public string? Parent { get; init; }
    public string? LiteralSyntax { get; init; }
    public string[] LiteralExamples { get; init; } = [];
}

public interface ITypeDescriptor
{
    Type? RuntimeType { get; }
}

public interface IExpressifValueType { }

public abstract class TypeDescriptor<T> : ITypeDescriptor
{
    public Type RuntimeType => typeof(T);
}

public static class TypeRegistry
{
    public static IReadOnlyList<TypeDescriptor> All { get; } = new TypeIntrospector().Describe().ToArray();

    private static readonly IReadOnlyDictionary<string, TypeDescriptor> ByName = BuildLookup();

    public static bool TryResolve(string name, out TypeDescriptor descriptor)
        => ByName.TryGetValue(name, out descriptor!);

    public static TypeDescriptor Resolve(string name)
        => TryResolve(name, out var descriptor)
            ? descriptor
            : throw new UnknownExpressifTypeException(name);

    private static IReadOnlyDictionary<string, TypeDescriptor> BuildLookup()
    {
        var lookup = All.ToDictionary(descriptor => descriptor.Name, StringComparer.OrdinalIgnoreCase);
        lookup.Add("date-time", lookup["datetime"]);
        return lookup;
    }
}

public static class RuntimeTypeRegistry
{
    private static readonly IReadOnlyDictionary<string, Type?> ByName = BuildLookup();

    public static Type? Resolve(string name)
        => ByName.TryGetValue(name, out var runtimeType)
            ? runtimeType
            : throw new UnknownExpressifTypeException(name);

    private static IReadOnlyDictionary<string, Type?> BuildLookup()
    {
        var lookup = typeof(RuntimeTypeRegistry).Assembly.GetTypes()
            .Where(type => !type.IsAbstract
                && type.IsDefined(typeof(ExpressifTypeAttribute), false)
                && (typeof(ITypeDescriptor).IsAssignableFrom(type)
                    || typeof(IExpressifValueType).IsAssignableFrom(type)))
            .ToDictionary(
                ExpressifTypeName.Get,
                GetRuntimeType,
                StringComparer.OrdinalIgnoreCase);
        lookup.Add("date-time", lookup["datetime"]);
        return lookup;
    }

    private static Type? GetRuntimeType(Type implementationType)
    {
        if (!typeof(ITypeDescriptor).IsAssignableFrom(implementationType))
            return implementationType;

        var descriptor = Activator.CreateInstance(implementationType) as ITypeDescriptor
            ?? throw new InvalidOperationException(
                $"Unable to create runtime type descriptor '{implementationType.FullName}'.");
        return descriptor.RuntimeType;
    }
}

public sealed class TypeIntrospector : BaseIntrospector
{
    public TypeIntrospector()
        : this(new AssemblyTypesProbe()) { }

    public TypeIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }

    public TypeIntrospector(ITypesProbe probe)
        : base(probe) { }

    public IEnumerable<TypeDescriptor> Describe()
        => Types
            .Where(type => typeof(ITypeDescriptor).IsAssignableFrom(type)
                || typeof(IExpressifValueType).IsAssignableFrom(type))
            .Where(type => type.IsDefined(typeof(ExpressifTypeAttribute), false))
            .Select(Describe)
            .OrderBy(descriptor => descriptor.Name);

    private static TypeDescriptor Describe(Type implementationType)
    {
        var metadata = implementationType.GetCustomAttribute<ExpressifTypeAttribute>()
            ?? throw new InvalidOperationException($"Type metadata is missing for '{implementationType.FullName}'.");
        var descriptor = typeof(ITypeDescriptor).IsAssignableFrom(implementationType)
            ? (ITypeDescriptor)Activator.CreateInstance(implementationType)!
            : null;
        var runtimeType = descriptor is null ? implementationType : descriptor.RuntimeType;

        return new TypeDescriptor(
            ExpressifTypeName.Get(implementationType),
            implementationType.GetSummary(),
            metadata.Parent,
            metadata.LiteralExamples.Length == 0
                ? null
                : new TypeLiteralMetadata(metadata.LiteralSyntax, metadata.LiteralExamples),
            runtimeType is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["dotnet"] = runtimeType.FullName! });
    }
}

internal static class ExpressifTypeName
{
    public static string Get(Type implementationType)
    {
        var metadata = implementationType.GetCustomAttribute<ExpressifTypeAttribute>()
            ?? throw new InvalidOperationException($"Type metadata is missing for '{implementationType.FullName}'.");
        if (metadata.Name is not null)
            return metadata.Name;

        var name = implementationType.Name;
        foreach (var suffix in new[] { "TypeDescriptor", "Value" })
        {
            if (!name.EndsWith(suffix, StringComparison.Ordinal))
                continue;

            name = name[..^suffix.Length];
            break;
        }
        return name.ToKebabCase();
    }
}

public sealed class UnknownExpressifTypeException : Exception
{
    public UnknownExpressifTypeException(string name)
        : base($"Unknown Expressif type literal ':{name}'.") { }
}
