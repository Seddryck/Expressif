using System.Collections;
using Expressif.Values;
using DictionaryValueType = Expressif.Values.Dictionary;

namespace Expressif.Library.Dictionary;

/// <summary>Restructures a dictionary with tuple keys into nested dictionaries, one level per tuple position, preserving key types and source insertion order.</summary>
[Function(prefix: "")]
[Scope("dictionary")]
public sealed class Nest : IFunction<DictionaryValueType, DictionaryValueType>
{
    private static readonly IEqualityComparer KeyComparer = StructuralComparisons.StructuralEqualityComparer;

    public DictionaryValueType Evaluate(DictionaryValueType value)
    {
        if (value.Count == 0)
            return new Expressif.Values.Dictionary([]);

        if (value[0].Key is not IPositionalValue first || first.Arity < 2)
            throw new ArgumentException("Every nest key must be a tuple of the same arity, at least two.", nameof(value));

        var entries = new List<(IPositionalValue Key, object? Value)>();
        var arity = first.Arity;
        foreach (var entry in value)
        {
            if (entry.Key is not IPositionalValue tuple || tuple.Arity != arity)
                throw new ArgumentException("Every nest key must be a tuple of the same arity, at least two.", nameof(value));
            entries.Add((tuple, entry.Value));
        }

        return Build(entries, 0, arity);
    }

    object? IFunction.Evaluate(object? value) => value is DictionaryValueType dictionary ? Evaluate(dictionary) : null;

    private static DictionaryValueType Build(IReadOnlyList<(IPositionalValue Key, object? Value)> entries, int position, int arity)
    {
        if (position == arity - 1)
            return new Expressif.Values.Dictionary(entries.Select(entry => new Expressif.Values.Pair(entry.Key.GetPosition(position), entry.Value)));

        var groups = new List<(object? Key, List<(IPositionalValue Key, object? Value)> Entries)>();
        foreach (var entry in entries)
        {
            var key = entry.Key.GetPosition(position);
            var index = groups.FindIndex(group => KeyComparer.Equals(group.Key, key));
            if (index < 0)
                groups.Add((key, [entry]));
            else
                groups[index].Entries.Add(entry);
        }

        return new Expressif.Values.Dictionary(groups.Select(group => new Expressif.Values.Pair(group.Key, Build(group.Entries, position + 1, arity))));
    }
}
