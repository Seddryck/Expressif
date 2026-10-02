namespace Expressif.Discovery;

/// <summary>
/// Identifies an Expressif operator independently from its implementation.
/// </summary>
public sealed record OperatorIdentity
{
    public OperatorIdentity(string @namespace, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (@namespace.Contains("::", StringComparison.Ordinal) || name.Contains("::", StringComparison.Ordinal))
            throw new ArgumentException("Operator identity components cannot contain '::'.");

        Namespace = Normalize(@namespace);
        Name = ImplementationRegistry.NormalizeName(name);
    }

    public string Namespace { get; }
    public string Name { get; }
    public string CanonicalName => $"{Namespace}::{Name}";

    public override string ToString() => CanonicalName;

    public static OperatorIdentity Parse(string canonicalName, string defaultNamespace = "global")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalName);
        var separator = canonicalName.IndexOf("::", StringComparison.Ordinal);
        return separator < 0
            ? new OperatorIdentity(defaultNamespace, canonicalName)
            : new OperatorIdentity(canonicalName[..separator], canonicalName[(separator + 2)..]);
    }

    internal static string NamespaceFromScope(string? scope)
        => string.IsNullOrWhiteSpace(scope) ? "global" : Normalize(scope.Split('/')[0]);

    internal static string NamespaceFromType(Type type)
    {
        var scope = type.GetCustomAttributes(typeof(Functions.ScopeAttribute), true)
            .OfType<Functions.ScopeAttribute>()
            .FirstOrDefault()?.Name;
        if (!string.IsNullOrWhiteSpace(scope))
            return NamespaceFromScope(scope);

        var segments = type.Namespace?.Split('.') ?? [];
        var library = Array.FindIndex(segments, segment => segment.Equals("Library", StringComparison.Ordinal));
        return library >= 0 && library + 1 < segments.Length
            ? Normalize(segments[library + 1])
            : "global";
    }

    private static string Normalize(string value) => value.Trim().ToKebabCase();
}
