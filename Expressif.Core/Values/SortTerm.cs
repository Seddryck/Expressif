using System.Collections;
using Expressif.Values.Types;

namespace Expressif.Values;

public sealed class SortComparer : IEquatable<SortComparer>
{
    private readonly Func<object?, object?, OrderingValue?> compare;

    internal SortComparer(string name, Type identity, Func<object?, object?, OrderingValue?> compare)
        => (Name, Identity, this.compare) = (name, identity, compare);

    public string Name { get; }
    internal Type Identity { get; }
    public OrderingValue? Compare(object? left, object? right) => compare(left, right);
    public bool Equals(SortComparer? other) => other is not null && Identity == other.Identity;
    public override bool Equals(object? obj) => Equals(obj as SortComparer);
    public override int GetHashCode() => Identity.GetHashCode();
    public override string ToString() => $"{Name}~";
}

/// <summary>Represents a value and the metadata needed to compare it while sorting.</summary>
[ExpressifType(Name = "sort-term", Parent = "tuple", LiteralSyntax = "SortTerm(value, comparer~, ascending, nulls-first)", LiteralExamples = ["SortTerm(42, compare-numeric~, #true, #false)"])]
public sealed class SortTerm : IReadOnlyList<object?>, IEquatable<SortTerm>, IExpressifValueType, IPositionalValue
{
    public SortTerm(object? value, SortComparer comparer, bool ascending, bool nullsFirst)
        => (Value, Comparer, Ascending, NullsFirst) =
            (value, comparer ?? throw new ArgumentNullException(nameof(comparer)), ascending, nullsFirst);

    public object? Value { get; }
    public SortComparer Comparer { get; }
    public bool Ascending { get; }
    public bool NullsFirst { get; }
    public int Count => 4;
    public int Arity => Count;
    public object? this[int index] => GetPosition(index);
    public object? GetPosition(int index) => index switch
    {
        0 => Value,
        1 => Comparer,
        2 => Ascending,
        3 => NullsFirst,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    public IEnumerator<object?> GetEnumerator() => Enumerable.Range(0, Count).Select(GetPosition).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool Equals(SortTerm? other) => other is not null && PositionalValueEquality.Equals(this, other);
    public override bool Equals(object? obj) => PositionalValueEquality.Equals(this, obj);
    public override int GetHashCode() => PositionalValueEquality.GetHashCode(this);
    public override string ToString() => ValueFormatter.Format(this);
}
