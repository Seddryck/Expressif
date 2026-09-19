using Expressif.Types;
using Expressif.Values.Casters;

namespace Expressif.Values;

/// <summary>Represents an immutable, fixed-size positional collection of numeric components.</summary>
[ExpressifType(Parent = "tuple", LiteralSyntax = "V followed by parenthesized comma-separated numeric values", LiteralExamples = ["V(1, 2, 3)"])]
public class VectorValue : TupleValue
{
    public VectorValue(params object?[] values)
        : base(Validate(values)) { }

    private static object?[] Validate(object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Any(value => value is null || !TypeChecker.IsNumericType(value)))
            throw new ArgumentException("Vector components must be numeric.", nameof(values));
        return values;
    }
}

/// <summary>Represents the public canonical vector value type.</summary>
public sealed class Vector : VectorValue
{
    public Vector(params object?[] values)
        : base(values) { }
}
