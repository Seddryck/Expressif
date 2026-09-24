namespace Expressif.Functions;

/// <summary>Describes whether a provider returns a stable or freshly constructed semantic object.</summary>
internal enum ProviderLifetime
{
    BoundExpression,
    FreshPerRequest,
}

/// <summary>
/// Declares the lifetime of a provider result, not the runtime function's invocation frequency.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ProviderLifetimeAttribute(ProviderLifetime lifetime) : Attribute
{
    public ProviderLifetime Lifetime { get; } = lifetime;
}
