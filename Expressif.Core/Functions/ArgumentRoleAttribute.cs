namespace Expressif.Functions;

/// <summary>Describes the semantic object supplied by a constructor argument.</summary>
internal enum ArgumentRole
{
    Predicate,
    Transformation,
    Accumulator,
}

/// <summary>
/// Declares the semantic provider supplied to a runtime constructor. Use a specialized
/// constructor when binding also needs structural validation or custom control flow.
/// Evaluation context, packing, and invocation frequency remain separate concerns.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ArgumentRoleAttribute(ArgumentRole role) : Attribute
{
    public ArgumentRole Role { get; } = role;
    public bool AllowValueExpression { get; set; }
}
