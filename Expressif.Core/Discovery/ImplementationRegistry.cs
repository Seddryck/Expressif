namespace Expressif.Discovery;

public sealed record ImplementationRegistration(string Name, Type ImplementationType)
{
    private string @namespace = "global";

    public ImplementationRegistration(string @namespace, string name, Type implementationType)
        : this(name, implementationType) => Namespace = new OperatorIdentity(@namespace, name).Namespace;

    public string Namespace
    {
        get => @namespace;
        init => @namespace = new OperatorIdentity(value, Name).Namespace;
    }
    public OperatorIdentity Identity => new(Namespace, Name);
}

public interface IImplementationRegistry
{
    Type Resolve(string name);
    bool TryResolve(string name, out Type implementationType);
    Type Resolve(OperatorIdentity identity) => Resolve(identity.CanonicalName);
    bool TryResolve(OperatorIdentity identity, out Type implementationType)
        => TryResolve(identity.CanonicalName, out implementationType);
}

public class ImplementationRegistry : IImplementationRegistry
{
    private readonly IReadOnlyDictionary<OperatorIdentity, Type> implementations;
    private readonly IReadOnlyDictionary<string, Type> unqualified;

    public ImplementationRegistry(IEnumerable<ImplementationRegistration> registrations)
        => (implementations, unqualified) = Build(registrations);

    public Type Resolve(string name)
        => TryResolve(name, out var implementationType)
            ? implementationType
            : throw new NotImplementedFunctionException(name);

    public bool TryResolve(string name, out Type implementationType)
    {
        if (name.Contains("::", StringComparison.Ordinal))
            return TryResolve(OperatorIdentity.Parse(name), out implementationType);
        return unqualified.TryGetValue(NormalizeName(name), out implementationType!);
    }

    public Type Resolve(OperatorIdentity identity)
        => TryResolve(identity, out var implementationType)
            ? implementationType
            : throw new NotImplementedFunctionException(identity.CanonicalName);

    public bool TryResolve(OperatorIdentity identity, out Type implementationType)
        => implementations.TryGetValue(identity, out implementationType!);

    internal static string NormalizeName(string name)
        => name.ToKebabCase().Replace("date-time", "dateTime", StringComparison.Ordinal);

    private static (IReadOnlyDictionary<OperatorIdentity, Type> Qualified, IReadOnlyDictionary<string, Type> Unqualified) Build(
        IEnumerable<ImplementationRegistration> registrations)
    {
        var registry = new Dictionary<OperatorIdentity, Type>();
        foreach (var registration in registrations)
        {
            if (registry.TryGetValue(registration.Identity, out var existing))
            {
                throw new InvalidOperationException(
                    $"The implementation name '{registration.Identity.CanonicalName}' is already registered for "
                    + $"'{existing.FullName}' and cannot also be registered for "
                    + $"'{registration.ImplementationType.FullName}'.");
            }
            registry.Add(registration.Identity, registration.ImplementationType);
        }

        var unqualified = registry
            .GroupBy(pair => pair.Key.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.First().Value, StringComparer.OrdinalIgnoreCase);
        return (registry, unqualified);
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
        Type? match = null;
        foreach (var registry in registries)
        {
            if (!registry.TryResolve(name, out var candidate))
                continue;
            if (match is not null)
            {
                implementationType = null!;
                return false;
            }
            match = candidate;
        }
        implementationType = match!;
        return match is not null;
    }

    public Type Resolve(OperatorIdentity identity)
        => TryResolve(identity, out var implementationType)
            ? implementationType
            : throw new NotImplementedFunctionException(identity.CanonicalName);

    public bool TryResolve(OperatorIdentity identity, out Type implementationType)
    {
        Type? match = null;
        foreach (var registry in registries)
        {
            if (!registry.TryResolve(identity, out var candidate))
                continue;
            if (match is not null && match != candidate)
            {
                throw new InvalidOperationException(
                    $"The implementation name '{identity.CanonicalName}' is registered for both "
                    + $"'{match.FullName}' and '{candidate.FullName}'.");
            }
            match = candidate;
        }
        implementationType = match!;
        return match is not null;
    }
}
