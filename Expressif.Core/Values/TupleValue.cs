using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Types;

namespace Expressif.Values;

/// <summary>
/// Represents an immutable, ordered collection of heterogeneous values.
/// </summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "T followed by parenthesized comma-separated values", LiteralExamples = ["T(\"Alice\", 42)"])]
public class TupleValue : IReadOnlyList<object?>, IEquatable<TupleValue>, IExpressifValueType, IPositionalValue
{
    private readonly object?[] values;

    public TupleValue(params object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.values = [.. values];
    }

    public int Count => values.Length;
    public object? this[int index] => values[index];
    public int Arity => Count;
    public object? GetPosition(int index) => this[index];

    public IEnumerator<object?> GetEnumerator()
        => ((IEnumerable<object?>)values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => values.GetEnumerator();

    public bool Equals(TupleValue? other)
        => other is not null && PositionalValueEquality.Equals(this, other);

    public override bool Equals(object? obj)
        => PositionalValueEquality.Equals(this, obj);

    public override int GetHashCode()
    {
        return PositionalValueEquality.GetHashCode(this);
    }

    public override string ToString()
        => ValueFormatter.Format(this);
}

/// <summary>
/// Represents the public canonical tuple value type.
/// </summary>
public sealed class Tuple : TupleValue
{
    public Tuple(params object?[] values)
        : base(values) { }
}
