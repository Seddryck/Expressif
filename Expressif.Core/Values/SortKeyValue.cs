using Expressif.Values.Types;

namespace Expressif.Values;

/// <summary>Represents a non-empty sequence of sort terms applied lexicographically.</summary>
[ExpressifType(Name = "sort-key", Parent = "tuple", LiteralSyntax = "SortKey followed by parenthesized comma-separated SortTerm values", LiteralExamples = ["SortKey(SortTerm(42, compare-numeric~, #true, #false))"])]
public class SortKeyValue : TupleValue
{
    public SortKeyValue(params SortTermValue[] terms)
        : base(Validate(terms).Cast<object?>().ToArray())
        => Terms = [.. terms];

    public IReadOnlyList<SortTermValue> Terms { get; }

    private static SortTermValue[] Validate(SortTermValue[] terms)
    {
        ArgumentNullException.ThrowIfNull(terms);
        if (terms.Length == 0)
            throw new ArgumentException("SortKey requires at least one SortTerm.", nameof(terms));
        if (terms.Any(term => term is null))
            throw new ArgumentException("SortKey values must all be SortTerm values.", nameof(terms));
        return terms;
    }
}

public sealed class SortKey : SortKeyValue
{
    public SortKey(params SortTermValue[] terms)
        : base(terms) { }
}
