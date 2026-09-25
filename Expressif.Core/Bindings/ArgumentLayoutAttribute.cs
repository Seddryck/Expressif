namespace Expressif.Bindings;

/// <summary>Describes the arrangement of supplied function arguments.</summary>
internal enum ArgumentLayoutKind
{
    Positional,
    Named,
    PositionalThenNamed,
}

/// <summary>
/// Declares reusable layout rules for specialized constructors. Value normalization and
/// function-specific cross-argument rules remain with the specialized constructor.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
internal sealed class ArgumentLayoutAttribute(ArgumentLayoutKind kind) : Attribute
{
    public ArgumentLayoutKind Kind { get; } = kind;
    public int MinimumCardinality { get; set; }
    public int MaximumCardinality { get; set; } = int.MaxValue;
    public int PositionalPrefix { get; set; }
    public bool RequireUniqueNames { get; set; }
}

internal sealed record ArgumentLayoutBinding(FunctionArgument[] Positional, FunctionArgument[] Named);
