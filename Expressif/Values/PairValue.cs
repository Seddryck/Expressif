using System;
using System.Collections;
using Expressif.Types;

namespace Expressif.Values;

/// <summary>
/// Represents an immutable key/value association.
/// </summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "Key and value expressions separated by => and enclosed in parentheses", LiteralExamples = ["(\"BE\" => 42)"])]
public class PairValue : IEquatable<PairValue>, IExpressifValueType
{
    private static readonly IEqualityComparer StructuralComparer = StructuralComparisons.StructuralEqualityComparer;

    public PairValue(object? key, object? value)
        => (Key, Value) = (key, value);

    public object? Key { get; }
    public object? Value { get; }

    public bool Equals(PairValue? other)
        => other is not null
            && StructuralComparer.Equals(Key, other.Key)
            && StructuralComparer.Equals(Value, other.Value);

    public override bool Equals(object? obj)
        => obj is PairValue other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            Key is null ? 0 : StructuralComparer.GetHashCode(Key),
            Value is null ? 0 : StructuralComparer.GetHashCode(Value));

    public override string ToString()
        => ValueFormatter.Format(this);
}

/// <summary>
/// Represents the public canonical pair value type.
/// </summary>
public sealed class Pair : PairValue
{
    public Pair(object? key, object? value)
        : base(key, value) { }
}
