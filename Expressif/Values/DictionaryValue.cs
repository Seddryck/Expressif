using System.Collections;
using Expressif.Types;

namespace Expressif.Values;

/// <summary>Represents an immutable ordered mapping with structurally unique keys.</summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "Pair entries enclosed in !{ and }", LiteralExamples = ["!{(\"BE\" => \"Belgium\")}"])]
public class DictionaryValue : IReadOnlyList<PairValue>, IEquatable<DictionaryValue>, IExpressifValueType
{
    private static readonly IEqualityComparer Comparer = StructuralComparisons.StructuralEqualityComparer;
    private readonly PairValue[] entries;

    public DictionaryValue(IEnumerable<PairValue> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var values = new List<PairValue>();
        foreach (var pair in entries)
        {
            if (values.Any(entry => Comparer.Equals(entry.Key, pair.Key)))
                throw new ArgumentException($"A dictionary cannot contain duplicate key '{ValueFormatter.Format(pair.Key)}'.", nameof(entries));
            values.Add(new Pair(pair.Key, pair.Value));
        }
        this.entries = values.ToArray();
    }

    public int Count => entries.Length;
    public PairValue this[int index] => entries[index];
    public IEnumerator<PairValue> GetEnumerator() => ((IEnumerable<PairValue>)entries).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => entries.GetEnumerator();
    public bool Equals(DictionaryValue? other) => other is not null && entries.SequenceEqual(other.entries);
    public override bool Equals(object? obj) => obj is DictionaryValue other && Equals(other);
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var entry in entries)
            hash.Add(entry);
        return hash.ToHashCode();
    }
    public override string ToString() => ValueFormatter.Format(this);
}

/// <summary>Represents the public canonical dictionary value type.</summary>
public sealed class Dictionary : DictionaryValue
{
    public Dictionary(IEnumerable<PairValue> entries)
        : base(entries) { }
}
