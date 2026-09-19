using Expressif.Functions.Introspection;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Expressif.Types;

public sealed record TypeLiteralMetadata(string? Syntax, string[] Examples);

public sealed record TypeDescriptor(
    string Name,
    string Summary,
    string? Parent,
    TypeLiteralMetadata? Literal,
    IReadOnlyDictionary<string, string> Bindings,
    Type? RuntimeType);

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

    public static bool IsInstance(object? value, TypeDescriptor expected)
    {
        ArgumentNullException.ThrowIfNull(expected);

        if (value is null)
            return expected.Name.Equals("null", StringComparison.OrdinalIgnoreCase);

        var actual = All
            .Where(descriptor => descriptor.RuntimeType?.IsInstanceOfType(value) == true)
            .OrderByDescending(GetDepth)
            .FirstOrDefault();

        while (actual is not null)
        {
            if (actual.Name.Equals(expected.Name, StringComparison.OrdinalIgnoreCase))
                return true;
            actual = actual.Parent is null ? null : Resolve(actual.Parent);
        }

        return false;
    }

    private static int GetDepth(TypeDescriptor descriptor)
    {
        var depth = 0;
        while (descriptor.Parent is not null)
        {
            depth++;
            descriptor = Resolve(descriptor.Parent);
        }
        return depth;
    }

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

public sealed class TypeIntrospector
{
    private readonly ITypesProbe probe;
    private Type[]? types;
    private Type[] Types => types ??= probe.Locate().ToArray();

    public TypeIntrospector()
        : this(new AssemblyTypesProbe()) { }

    public TypeIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }

    public TypeIntrospector(ITypesProbe probe)
        => this.probe = probe;

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
            TypeDocumentation.GetSummary(implementationType),
            metadata.Parent,
            metadata.LiteralExamples.Length == 0
                ? null
                : new TypeLiteralMetadata(metadata.LiteralSyntax, metadata.LiteralExamples),
            runtimeType is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["dotnet"] = runtimeType.FullName! },
            runtimeType);
    }
}

internal static class TypeDocumentation
{
    private static readonly Dictionary<Assembly, XmlDocument> Cache = [];
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    public static string GetSummary(Type type)
    {
        var document = GetDocument(type.Assembly);
        var memberName = $"T:{type.FullName}";
        var summary = document["doc"]?["members"]?
            .SelectSingleNode($"member[@name='{memberName}']/summary");
        if (summary is null)
            return string.Empty;

        var text = Regex.Replace(
            summary.InnerXml,
            "<see\\s+langword\\s*=\\s*\"([^\"]+)\"\\s*/>",
            "`$1`",
            RegexOptions.IgnoreCase,
            RegexTimeout);
        text = Regex.Replace(text, "<.*?>", string.Empty, RegexOptions.None, RegexTimeout);
        return Regex.Replace(
            WebUtility.HtmlDecode(text),
            "\\s+",
            " ",
            RegexOptions.None,
            RegexTimeout).Trim();
    }

    private static XmlDocument GetDocument(Assembly assembly)
    {
        if (Cache.TryGetValue(assembly, out var document))
            return document;

        var path = Path.ChangeExtension(assembly.Location, ".xml");
        document = new XmlDocument();
        document.Load(path);
        Cache[assembly] = document;
        return document;
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
