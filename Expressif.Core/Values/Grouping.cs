using System.Collections;
using Expressif.Values.Types;

namespace Expressif.Values;

/// <summary>Represents one immutable grouping bucket.</summary>
[ExpressifType(Parent = "pair")]
public sealed class Group : IReadOnlyList<object?>, IEquatable<Group>, IExpressifValueType, IPositionalValue
{
    public Group(object? key, IEnumerable values)
        => (Key, Values) = (key, Materialize(values));

    public object? Key { get; }
    public IReadOnlyList<object?> Values { get; }
    public object? Value => Values;
    public int Count => Values.Count;
    public object? this[int index] => Values[index];
    public int Arity => 2;
    public object? GetPosition(int index) => index switch
    {
        0 => Key,
        1 => Value,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    public IEnumerator<object?> GetEnumerator() => Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool Equals(Group? other) => other is not null && PositionalValueEquality.Equals(this, other);
    public override bool Equals(object? obj) => PositionalValueEquality.Equals(this, obj);
    public override int GetHashCode() => PositionalValueEquality.GetHashCode(this);
    public override string ToString() => ValueFormatter.Format(this);

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

    public Grouping(IEnumerable<IPositionalValue> pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);
        var values = new List<Group>();
        foreach (var pair in pairs)
        {
            if (pair.Arity != 2)
                throw new ArgumentException("Every grouping entry must contain a key and a value.", nameof(pairs));
            var key = pair.GetPosition(0);
            var value = pair.GetPosition(1);
            if (values.Any(group => StructuralComparer.Equals(group.Key, key)))
                throw new ArgumentException($"A grouping cannot contain duplicate key '{ValueFormatter.Format(key)}'.", nameof(pairs));
            if (value is not IEnumerable collection || value is string)
                throw new ArgumentException("Every grouping entry value must be a collection.", nameof(pairs));
            values.Add(new Group(key, collection));
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
