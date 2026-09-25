namespace Expressif.Introspection;

/// <summary>Describes one source-to-target coercion available to Expressif.</summary>
public sealed class CoercionInfo
{
    internal CoercionInfo(string name, Type sourceType, Type targetType, Type implementationType)
        => (Name, SourceType, TargetType, ImplementationType) = (
            name,
            sourceType,
            targetType,
            implementationType);

    public string Name { get; }
    public Type SourceType { get; }
    public Type TargetType { get; }
    public Type ImplementationType { get; }
}
