using System.Collections;
using System.Collections.ObjectModel;

namespace Expressif.Collections;

internal sealed class StructuralReadOnlyList<T> : IReadOnlyList<T>, IEquatable<StructuralReadOnlyList<T>>
{
    private readonly T[] values;

    private StructuralReadOnlyList(IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.values = values.ToArray();
    }

    public static IReadOnlyList<T> Create(IEnumerable<T> values) => new StructuralReadOnlyList<T>(values);
    public int Count => values.Length;
    public T this[int index] => values[index];
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)values).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool Equals(StructuralReadOnlyList<T>? other)
        => other is not null && values.SequenceEqual(other.values);
    public override bool Equals(object? obj) => obj is StructuralReadOnlyList<T> other && Equals(other);
    public override int GetHashCode()
    {
        HashCode hash = default;
        foreach (var value in values)
            hash.Add(value);
        return hash.ToHashCode();
    }
}

internal sealed class StructuralReadOnlyDictionary<TKey, TValue> :
    IReadOnlyDictionary<TKey, TValue>,
    IEquatable<StructuralReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly IReadOnlyDictionary<TKey, TValue> values;

    private StructuralReadOnlyDictionary(
        IEnumerable<KeyValuePair<TKey, TValue>> values,
        IEqualityComparer<TKey>? comparer)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.values = new ReadOnlyDictionary<TKey, TValue>(values.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            comparer ?? EqualityComparer<TKey>.Default));
    }

    public static IReadOnlyDictionary<TKey, TValue> Create(
        IEnumerable<KeyValuePair<TKey, TValue>> values,
        IEqualityComparer<TKey>? comparer = null)
        => new StructuralReadOnlyDictionary<TKey, TValue>(values, comparer);

    public int Count => values.Count;
    public IEnumerable<TKey> Keys => values.Keys;
    public IEnumerable<TValue> Values => values.Values;
    public TValue this[TKey key] => values[key];
    public bool ContainsKey(TKey key) => values.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => values.TryGetValue(key, out value!);
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(StructuralReadOnlyDictionary<TKey, TValue>? other)
    {
        if (other is null || Count != other.Count)
            return false;
        foreach (var pair in values)
        {
            if (!other.TryGetValue(pair.Key, out var value)
                || !EqualityComparer<TValue>.Default.Equals(pair.Value, value))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj)
        => obj is StructuralReadOnlyDictionary<TKey, TValue> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var pair in values)
            hash ^= HashCode.Combine(pair.Key, pair.Value);
        return hash;
    }
}
