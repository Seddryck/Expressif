using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Types;

namespace Expressif.Values;

/// <summary>
/// Represents a value containing an ordered collection of named fields.
/// </summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "Named fields enclosed in braces", LiteralExamples = ["{name := \"Alice\", age := 42}"])]
public class RecordValue : IReadOnlyDictionary<string, object?>, IExpressifValueType
{
    private readonly List<string> order = [];
    private readonly Dictionary<string, object?> fields = [];

    public int Count => fields.Count;

    public IEnumerable<string> Keys => order;

    public IEnumerable<object?> Values
    {
        get
        {
            foreach (var key in order)
                yield return fields[key];
        }
    }

    public object? this[string key] => fields[key];

    public object? this[int index] => fields[order[index]];

    public bool ContainsKey(string key)
        => fields.ContainsKey(key);

    public bool TryGetValue(string key, out object? value)
        => fields.TryGetValue(key, out value);

    public void Set(string key, object? value)
    {
        if (!fields.ContainsKey(key))
            order.Add(key);

        fields[key] = value;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        foreach (var key in order)
            yield return new KeyValuePair<string, object?>(key, fields[key]);
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public override string ToString()
        => ValueFormatter.Format(this);
}
