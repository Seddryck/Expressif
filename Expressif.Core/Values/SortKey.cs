using System.Collections;
using Expressif.Values.Types;

namespace Expressif.Values;

/// <summary>Represents a non-empty sequence of sort terms applied lexicographically.</summary>
[ExpressifType(Name = "sort-key", Parent = "tuple", LiteralSyntax = "SortKey followed by parenthesized comma-separated SortTerm values", LiteralExamples = ["SortKey(SortTerm(42, compare-numeric~, #true, #false))"])]
public sealed class SortKey : IReadOnlyList<object?>, IEquatable<SortKey>, IExpressifValueType, IPositionalValue
{
    public SortKey(params SortTerm[] terms)
        => Terms = [.. Validate(terms)];

    public IReadOnlyList<SortTerm> Terms { get; }
    public int Count => Terms.Count;
    public int Arity => Count;
    public object? this[int index] => Terms[index];
    public object? GetPosition(int index) => this[index];
    public IEnumerator<object?> GetEnumerator() => Terms.Cast<object?>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool Equals(SortKey? other) => other is not null && PositionalValueEquality.Equals(this, other);
    public override bool Equals(object? obj) => PositionalValueEquality.Equals(this, obj);
    public override int GetHashCode() => PositionalValueEquality.GetHashCode(this);
    public override string ToString() => ValueFormatter.Format(this);

    private static SortTerm[] Validate(SortTerm[] terms)
    {
        ArgumentNullException.ThrowIfNull(terms);
        if (terms.Length == 0)
            throw new ArgumentException("SortKey requires at least one SortTerm.", nameof(terms));
        if (terms.Any(term => term is null))
            throw new ArgumentException("SortKey values must all be SortTerm values.", nameof(terms));
        return terms;
    }
}
