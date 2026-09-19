using System.Collections;
using Expressif.Types;

namespace Expressif.Values;

/// <summary>Represents one immutable grouping bucket.</summary>
[ExpressifType(Parent = "pair")]
public sealed class Group : PairValue, IReadOnlyList<object?>
{
    public Group(object? key, IEnumerable values)
        : base(key, Materialize(values)) { }

    public IReadOnlyList<object?> Values => (IReadOnlyList<object?>)Value!;
    public int Count => Values.Count;
    public object? this[int index] => Values[index];

    public IEnumerator<object?> GetEnumerator() => Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static object?[] Materialize(IEnumerable values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values is string)
            throw new ArgumentException("A group value must be a collection.", nameof(values));
        return values.Cast<object?>().ToArray();
    }
}

/// <summary>Represents an immutable ordered collection of groups with unique keys.</summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "Pair entries enclosed in #{ and }", LiteralExamples = ["#{(\"BE\" => {\"alice\", \"bob\"})}"])]
public sealed class Grouping : IReadOnlyList<Group>, IEquatable<Grouping>, IExpressifValueType
{
    private static readonly IEqualityComparer StructuralComparer = StructuralComparisons.StructuralEqualityComparer;
    private readonly Group[] groups;

    public Grouping(IEnumerable<PairValue> pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);
        var values = new List<Group>();
        foreach (var pair in pairs)
        {
            if (values.Any(group => StructuralComparer.Equals(group.Key, pair.Key)))
                throw new ArgumentException($"A grouping cannot contain duplicate key '{ValueFormatter.Format(pair.Key)}'.", nameof(pairs));
            if (pair.Value is not IEnumerable collection || pair.Value is string)
                throw new ArgumentException("Every grouping entry value must be a collection.", nameof(pairs));
            values.Add(new Group(pair.Key, collection));
        }
        groups = values.ToArray();
    }

    public int Count => groups.Length;
    public Group this[int index] => groups[index];

    public IEnumerator<Group> GetEnumerator() => ((IEnumerable<Group>)groups).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => groups.GetEnumerator();

    public bool Equals(Grouping? other) => other is not null && groups.SequenceEqual(other.groups);
    public override bool Equals(object? obj) => obj is Grouping other && Equals(other);

    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var group in groups)
            hash.Add(group);
        return hash.ToHashCode();
    }

    public override string ToString() => ValueFormatter.Format(this);
}
