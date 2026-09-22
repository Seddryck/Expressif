using System.Collections;
using Expressif.Values.Types;
using Expressif.Values.Casters;

namespace Expressif.Values;

/// <summary>Represents an immutable, fixed-size positional collection of numeric components.</summary>
[ExpressifType(Parent = "tuple", LiteralSyntax = "V followed by parenthesized comma-separated numeric values", LiteralExamples = ["V(1, 2, 3)"])]
public sealed class Vector : IReadOnlyList<object?>, IEquatable<Vector>, IExpressifValueType, IPositionalValue
{
    private readonly object?[] values;

    public Vector(params object?[] values)
        => this.values = Validate(values);

    public int Count => values.Length;
    public object? this[int index] => values[index];
    public int Arity => Count;
    public object? GetPosition(int index) => this[index];

    public IEnumerator<object?> GetEnumerator()
        => ((IEnumerable<object?>)values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => values.GetEnumerator();

    public bool Equals(Vector? other)
        => other is not null && PositionalValueEquality.Equals(this, other);

    public override bool Equals(object? obj)
        => PositionalValueEquality.Equals(this, obj);

    public override int GetHashCode()
        => PositionalValueEquality.GetHashCode(this);

    public override string ToString()
        => ValueFormatter.Format(this);

    private static object?[] Validate(object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Any(value => value is null || !TypeChecker.IsNumericType(value)))
            throw new ArgumentException("Vector components must be numeric.", nameof(values));
        return values;
    }
}
