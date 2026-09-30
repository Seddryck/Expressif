using System.Collections;
using Expressif.Values.Types;

namespace Expressif.Values;

public sealed record SortHeader(SortComparer Comparer, bool Ascending, bool NullsFirst);
public sealed record SortRow(IReadOnlyList<object?> Keys, object? Value);

/// <summary>Represents normalized sort metadata and rows.</summary>
[ExpressifType(Name = "sort-table", Parent = "record", LiteralSyntax = "SortTable{headers := {...}, rows := {...}}", LiteralExamples = ["SortTable{headers := {}, rows := {}}"])]
public sealed class SortTableValue : IReadOnlyDictionary<string, object?>, IExpressifValueType
{
    private readonly IReadOnlyDictionary<string, object?> fields;

    public SortTableValue(IEnumerable<SortHeader> headers, IEnumerable<SortRow> rows)
    {
        Headers = headers?.ToArray() ?? throw new ArgumentNullException(nameof(headers));
        Rows = rows?.ToArray() ?? throw new ArgumentNullException(nameof(rows));
        fields = new Dictionary<string, object?>
        {
            ["headers"] = Headers.Select(header => new Tuple(header.Comparer, header.Ascending, header.NullsFirst)).ToArray(),
            ["rows"] = Rows.Select(row => new Tuple(new Tuple(row.Keys.ToArray()), row.Value)).ToArray(),
        };
    }

    public IReadOnlyList<SortHeader> Headers { get; }
    public IReadOnlyList<SortRow> Rows { get; }
    public int Count => fields.Count;
    public IEnumerable<string> Keys => fields.Keys;
    public IEnumerable<object?> Values => fields.Values;
    public object? this[string key] => fields[key];
    public bool ContainsKey(string key) => fields.ContainsKey(key);
    public bool TryGetValue(string key, out object? value) => fields.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => fields.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public override string ToString() => ValueFormatter.Format(this);
}
