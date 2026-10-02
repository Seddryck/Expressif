namespace Expressif.Functions;

/// <summary>
/// Describes the scope supplied to a runtime constructor callback.
/// </summary>
internal enum ArgumentEvaluationMode
{
    Ambient,
    Incoming,
    Nested,
}

/// <summary>
/// Makes callback evaluation semantics explicit for generic function construction.
/// The runtime function, not this metadata, determines invocation frequency.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ArgumentEvaluationAttribute(ArgumentEvaluationMode mode) : Attribute
{
    public ArgumentEvaluationMode Mode { get; } = mode;
}
