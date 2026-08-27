using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Expressif.Values;

/// <summary>
/// Represents an immutable, ordered collection of heterogeneous values.
/// </summary>
public class TupleValue : IReadOnlyList<object?>, IEquatable<TupleValue>
{
    private readonly object?[] values;

    public TupleValue(params object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.values = [.. values];
    }

    public int Count => values.Length;
    public object? this[int index] => values[index];

    public IEnumerator<object?> GetEnumerator()
        => ((IEnumerable<object?>)values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => values.GetEnumerator();

    public bool Equals(TupleValue? other)
        => other is not null && values.SequenceEqual(other.values);

    public override bool Equals(object? obj)
        => obj is TupleValue other && Equals(other);

    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var value in values)
            hash.Add(value);
        return hash.ToHashCode();
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
