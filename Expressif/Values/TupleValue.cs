using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expressif.Types;

namespace Expressif.Values;

/// <summary>
/// Represents an immutable, ordered collection of heterogeneous values.
/// </summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "T followed by parenthesized comma-separated values", LiteralExamples = ["T(\"Alice\", 42)"])]
public class TupleValue : IReadOnlyList<object?>, IEquatable<TupleValue>, IExpressifValueType
{
    private static readonly IEqualityComparer<object?> StructuralComparer = new StructuralValueComparer();
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
        => other is not null && values.SequenceEqual(other.values, StructuralComparer);

    public override bool Equals(object? obj)
        => obj is TupleValue other && Equals(other);

    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var value in values)
            hash.Add(value, StructuralComparer);
        return hash.ToHashCode();
    }

    public override string ToString()
        => ValueFormatter.Format(this);

    private sealed class StructuralValueComparer : IEqualityComparer<object?>
    {
        public new bool Equals(object? x, object? y)
            => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

        public int GetHashCode(object? obj)
            => obj is null ? 0 : StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }
}

/// <summary>
/// Represents the public canonical tuple value type.
/// </summary>
public sealed class Tuple : TupleValue
{
    public Tuple(params object?[] values)
        : base(values) { }
}
