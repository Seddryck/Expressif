using System.Collections.ObjectModel;
using System.Reflection;

namespace Expressif.Discovery;

/// <summary>
/// Supplies types to implementation registries, factories, and introspectors.
/// </summary>
/// <remarks>
/// <para>
/// Consumers can call <see cref="GetTypes"/> and enumerate its result multiple times and concurrently.
/// Implementations must return a non-<see langword="null"/> sequence containing no <see langword="null"/>
/// elements and must document whether they return a cached snapshot or live results.
/// </para>
/// <para>
/// A type source supplies types without deciding whether they are concrete implementation candidates.
/// Consumers are responsible for filtering the returned types. Ordering has no semantic significance,
/// and consumers must not depend on one type source using the same ordering strategy as another.
/// </para>
/// </remarks>
public interface ITypeSource
{
    /// <summary>
    /// Gets the types available from this source.
    /// </summary>
    IEnumerable<Type> GetTypes();
}

/// <summary>
/// Supplies a cached snapshot of every loadable type in a set of assemblies.
/// </summary>
/// <remarks>
/// <para>
/// Duplicate assemblies and types are removed. Assembly input order is preserved, and types within each
/// assembly are ordered by assembly-qualified name. The resulting immutable snapshot is created on the first
/// call to <see cref="GetTypes"/>, cached, and safely shared by concurrent callers and enumerations.
/// </para>
/// <para>
/// When an assembly throws <see cref="ReflectionTypeLoadException"/>, its non-<see langword="null"/> loaded
/// types are included and its loader exceptions are ignored. Other failures are propagated and cached by the
/// source. No filtering for concrete classes or Expressif implementation roles is performed.
/// </para>
/// </remarks>
public sealed class AssemblyTypeSource : ITypeSource
{
    private readonly Lazy<ReadOnlyCollection<Type>> types;

    public AssemblyTypeSource()
        : this(typeof(IExpression).Assembly) { }

    public AssemblyTypeSource(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        if (assemblies.Any(assembly => assembly is null))
            throw new ArgumentException("Assemblies cannot contain null elements.", nameof(assemblies));

        var distinctAssemblies = assemblies.Distinct<Assembly>(ReferenceEqualityComparer.Instance).ToArray();
        types = new Lazy<ReadOnlyCollection<Type>>(
            () => LoadTypes(distinctAssemblies),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IEnumerable<Type> GetTypes() => types.Value;

    private static ReadOnlyCollection<Type> LoadTypes(IEnumerable<Assembly> assemblies)
    {
        var discovered = new List<Type>();
        var unique = new HashSet<Type>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in GetLoadableTypes(assembly)
                .OrderBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal))
            {
                if (unique.Add(type))
                    discovered.Add(type);
            }
        }
        return discovered.AsReadOnly();
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>();
        }
    }
}
