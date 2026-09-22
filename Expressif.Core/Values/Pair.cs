using System;
using Expressif.Values.Types;

namespace Expressif.Values;

/// <summary>
/// Represents an immutable key/value association and specialized tuple of arity two.
/// </summary>
[ExpressifType(Parent = "tuple", LiteralSyntax = "Key and value expressions separated by => and enclosed in parentheses", LiteralExamples = ["(\"BE\" => 42)"])]
public sealed class Pair : IEquatable<Pair>, IExpressifValueType, IPositionalValue
{
    public Pair(object? key, object? value)
        => (Key, Value) = (key, value);

    public object? Key { get; }
    public object? Value { get; }
    public int Arity => 2;
    public object? GetPosition(int index)
        => index switch
        {
            0 => Key,
            1 => Value,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public bool Equals(Pair? other)
        => other is not null && PositionalValueEquality.Equals(this, other);

    public override bool Equals(object? obj)
        => PositionalValueEquality.Equals(this, obj);

    public override int GetHashCode()
        => PositionalValueEquality.GetHashCode(this);

    public override string ToString()
        => ValueFormatter.Format(this);
}
