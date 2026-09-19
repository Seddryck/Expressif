using Expressif.Types;

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
public class SortTermValue : TupleValue
{
    public SortTermValue(object? value, SortComparer comparer, bool ascending, bool nullsFirst)
        : base(value, comparer ?? throw new ArgumentNullException(nameof(comparer)), ascending, nullsFirst)
        => (Value, Comparer, Ascending, NullsFirst) = (value, comparer, ascending, nullsFirst);

    public object? Value { get; }
    public SortComparer Comparer { get; }
    public bool Ascending { get; }
    public bool NullsFirst { get; }
}

public sealed class SortTerm : SortTermValue
{
    public SortTerm(object? value, SortComparer comparer, bool ascending, bool nullsFirst)
        : base(value, comparer, ascending, nullsFirst) { }
}
