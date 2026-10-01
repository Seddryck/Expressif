using System.Reflection;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Library.Catalog;
using Expressif.Types;
using Expressif.Values.Types;

namespace Expressif.Hosting;

/// <summary>Declares an independently versioned Expressif library assembly.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ExpressifLibraryAttribute(
    string name,
    string version,
    string minimumCoreApi,
    string maximumCoreApiExclusive) : Attribute
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string MinimumCoreApi { get; } = minimumCoreApi;
    public string MaximumCoreApiExclusive { get; } = maximumCoreApiExclusive;
}

/// <summary>Declares a required Expressif library and its supported version interval.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ExpressifLibraryDependencyAttribute(
    string name,
    string minimumVersion,
    string maximumVersionExclusive) : Attribute
{
    public string Name { get; } = name;
    public string MinimumVersion { get; } = minimumVersion;
    public string MaximumVersionExclusive { get; } = maximumVersionExclusive;
}

public sealed record ExpressifLibraryDependency(
    string Name,
    Version MinimumVersion,
    Version MaximumVersionExclusive);

public sealed record ExpressifLibraryInfo(
    string Name,
    Version Version,
    Version MinimumCoreApi,
    Version MaximumCoreApiExclusive,
    Assembly Assembly,
    IReadOnlyList<ExpressifLibraryDependency> Dependencies);

/// <summary>Represents an immutable set of registered Expressif libraries and their capabilities.</summary>
public sealed class ExpressifEnvironment
{
    private const string BuiltInLibraryName = "Expressif.Library";
    private static readonly Version BuiltInLibraryVersion = new(3, 0, 0);
    private static readonly Lazy<ExpressifEnvironment> LazyDefault = new(() => Create([]));

    private ExpressifEnvironment(
        IReadOnlyList<ExpressifLibraryInfo> libraries,
        ITypeSource source,
        ITypeRegistry types,
        QuotedLiteralRegistry quotedLiterals,
        FunctionCatalog catalog)
        => (Libraries, Source, Types, QuotedLiterals, Catalog) =
            (libraries, source, types, quotedLiterals, catalog);

    public static Version CoreApiVersion { get; } = new(3, 0, 0);
    public static ExpressifEnvironment Default => LazyDefault.Value;
    public IReadOnlyList<ExpressifLibraryInfo> Libraries { get; }
    public ITypeRegistry Types { get; }
    public QuotedLiteralRegistry QuotedLiterals { get; }
    public FunctionCatalog Catalog { get; }
    internal ITypeSource Source { get; }

    /// <summary>Validates and atomically adds a library assembly, returning a new environment.</summary>
    public ExpressifEnvironment RegisterLibrary(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        if (Libraries.Any(library => ReferenceEquals(library.Assembly, assembly)))
            return this;
        try
        {
            return Create(Libraries
                .Where(library => !library.Name.Equals(BuiltInLibraryName, StringComparison.OrdinalIgnoreCase))
                .Select(library => library.Assembly)
                .Append(assembly));
        }
        catch (LibraryRegistrationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new LibraryRegistrationException(
                $"Library assembly '{assembly.GetName().Name}' could not be registered: {exception.Message}",
                exception);
        }
    }

    /// <summary>Validates and atomically adds the assembly containing <typeparamref name="T"/>.</summary>
    public ExpressifEnvironment RegisterLibrary<T>()
        => RegisterLibrary(typeof(T).Assembly);

    private static ExpressifEnvironment Create(IEnumerable<Assembly> extensions)
    {
        var builtInAssembly = typeof(ExpressifEnvironment).Assembly;
        var extensionAssemblies = extensions.Distinct().ToArray();
        var extensionLibraries = extensionAssemblies.Select(ReadManifest).ToArray();
        var libraries = new[]
        {
            new ExpressifLibraryInfo(
                BuiltInLibraryName,
                BuiltInLibraryVersion,
                CoreApiVersion,
                new Version(CoreApiVersion.Major + 1, 0, 0),
                builtInAssembly,
                Array.Empty<ExpressifLibraryDependency>()),
        }.Concat(extensionLibraries).ToArray();

        ValidateIdentities(libraries);
        ValidateCompatibility(extensionLibraries);
        ValidateDependencies(libraries);

        var assemblies = new[] { builtInAssembly }.Concat(extensionAssemblies).ToArray();
        var source = new CompositeTypeSource(assemblies.Select(assembly => new AssemblyTypeSource(assembly)));
        var types = new TypeRegistry(
            new CompositeTypeSource(source, new AssemblyTypeSource(typeof(IExpression).Assembly)));
        var literals = QuotedLiteralRegistry.Default.Add(extensionAssemblies.SelectMany(DiscoverLiteralParsers));
        var catalog = FunctionCatalog.Load(assemblies);

        _ = new FunctionFactory(source);
        return new ExpressifEnvironment(libraries, source, types, literals, catalog);
    }

    private static ExpressifLibraryInfo ReadManifest(Assembly assembly)
    {
        var manifest = assembly.GetCustomAttribute<ExpressifLibraryAttribute>()
            ?? throw new LibraryRegistrationException(
                $"Assembly '{assembly.GetName().Name}' does not declare ExpressifLibraryAttribute.");
        return new ExpressifLibraryInfo(
            RequireName(manifest.Name, assembly),
            ParseVersion(manifest.Version, "library version", assembly),
            ParseVersion(manifest.MinimumCoreApi, "minimum core API version", assembly),
            ParseVersion(manifest.MaximumCoreApiExclusive, "maximum core API version", assembly),
            assembly,
            assembly.GetCustomAttributes<ExpressifLibraryDependencyAttribute>()
                .Select(dependency => new ExpressifLibraryDependency(
                    dependency.Name,
                    ParseVersion(dependency.MinimumVersion, "dependency minimum version", assembly),
                    ParseVersion(dependency.MaximumVersionExclusive, "dependency maximum version", assembly)))
                .ToArray());
    }

    private static IEnumerable<IQuotedLiteralParser> DiscoverLiteralParsers(Assembly assembly)
        => new AssemblyTypeSource(assembly).GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(IQuotedLiteralParser).IsAssignableFrom(type))
            .Select(type => (IQuotedLiteralParser)(Activator.CreateInstance(type)
                ?? throw new LibraryRegistrationException(
                    $"Quoted literal parser '{type.FullName}' could not be created.")));

    private static void ValidateIdentities(IReadOnlyList<ExpressifLibraryInfo> libraries)
    {
        var duplicate = libraries.GroupBy(library => library.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new LibraryRegistrationException($"Library '{duplicate.Key}' is already registered.");
    }

    private static void ValidateCompatibility(IEnumerable<ExpressifLibraryInfo> libraries)
    {
        foreach (var library in libraries)
        {
            if (library.MinimumCoreApi >= library.MaximumCoreApiExclusive)
            {
                throw new LibraryRegistrationException(
                    $"Library '{library.Name}' declares an empty core API compatibility interval.");
            }
            if (CoreApiVersion < library.MinimumCoreApi || CoreApiVersion >= library.MaximumCoreApiExclusive)
            {
                throw new LibraryRegistrationException(
                    $"Library '{library.Name}' version '{library.Version}' requires Expressif core API "
                    + $"[{library.MinimumCoreApi}, {library.MaximumCoreApiExclusive}), but the host provides "
                    + $"'{CoreApiVersion}'.");
            }
        }
    }

    private static void ValidateDependencies(IReadOnlyList<ExpressifLibraryInfo> libraries)
    {
        foreach (var library in libraries)
        {
            foreach (var dependency in library.Dependencies)
            {
                var registered = libraries.SingleOrDefault(candidate => candidate.Name.Equals(
                    dependency.Name,
                    StringComparison.OrdinalIgnoreCase));
                if (registered is null)
                {
                    throw new LibraryRegistrationException(
                        $"Library '{library.Name}' requires missing library '{dependency.Name}'.");
                }
                if (registered.Version < dependency.MinimumVersion
                    || registered.Version >= dependency.MaximumVersionExclusive)
                {
                    throw new LibraryRegistrationException(
                        $"Library '{library.Name}' requires '{dependency.Name}' in version interval "
                        + $"[{dependency.MinimumVersion}, {dependency.MaximumVersionExclusive}), but version "
                        + $"'{registered.Version}' is registered.");
                }
            }
        }
    }

    private static string RequireName(string name, Assembly assembly)
        => string.IsNullOrWhiteSpace(name)
            ? throw new LibraryRegistrationException(
                $"Assembly '{assembly.GetName().Name}' declares an empty library name.")
            : name.Trim();

    private static Version ParseVersion(string value, string description, Assembly assembly)
        => Version.TryParse(value, out var version)
            ? version
            : throw new LibraryRegistrationException(
                $"Assembly '{assembly.GetName().Name}' declares invalid {description} '{value}'.");

    private sealed class CompositeTypeSource(params ITypeSource[] sources) : ITypeSource
    {
        public CompositeTypeSource(IEnumerable<ITypeSource> sources)
            : this(sources.ToArray()) { }

        public IEnumerable<Type> GetTypes()
            => sources.SelectMany(source => source.GetTypes()).Distinct();
    }
}

public sealed class LibraryRegistrationException : Exception
{
    public LibraryRegistrationException(string message)
        : base(message) { }

    public LibraryRegistrationException(string message, Exception innerException)
        : base(message, innerException) { }
}
