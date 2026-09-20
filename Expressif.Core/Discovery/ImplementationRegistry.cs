namespace Expressif.Discovery;

public sealed record ImplementationRegistration(string Name, Type ImplementationType);

public interface IImplementationRegistry
{
    Type Resolve(string name);
    bool TryResolve(string name, out Type implementationType);
}

public class ImplementationRegistry : IImplementationRegistry
{
    private readonly IReadOnlyDictionary<string, Type> implementations;

    public ImplementationRegistry(IEnumerable<ImplementationRegistration> registrations)
        => implementations = Build(registrations);

    public Type Resolve(string name)
        => TryResolve(name, out var implementationType)
            ? implementationType
            : throw new NotImplementedFunctionException(name);

    public bool TryResolve(string name, out Type implementationType)
        => implementations.TryGetValue(NormalizeName(name), out implementationType!);

    public static string NormalizeName(string name)
        => name.ToKebabCase().Replace("date-time", "dateTime", StringComparison.Ordinal);

    private static IReadOnlyDictionary<string, Type> Build(
        IEnumerable<ImplementationRegistration> registrations)
    {
        var registry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var registration in registrations)
        {
            var name = NormalizeName(registration.Name);
            if (registry.TryGetValue(name, out var existing))
            {
                throw new InvalidOperationException(
                    $"The implementation name '{registration.Name}' is already registered for "
                    + $"'{existing.FullName}' and cannot also be registered for "
                    + $"'{registration.ImplementationType.FullName}'.");
            }
            registry.Add(name, registration.ImplementationType);
        }
        return registry;
    }
}

public sealed class CompositeImplementationRegistry(
    params IImplementationRegistry[] registries) : IImplementationRegistry
{
    public Type Resolve(string name)
        => TryResolve(name, out var implementationType)
            ? implementationType
            : throw new NotImplementedFunctionException(name);

    public bool TryResolve(string name, out Type implementationType)
    {
        foreach (var registry in registries)
        {
            if (registry.TryResolve(name, out implementationType))
                return true;
        }
        implementationType = null!;
        return false;
    }
}
