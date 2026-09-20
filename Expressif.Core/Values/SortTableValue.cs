using Expressif.Values.Types;

namespace Expressif.Values;

public sealed record SortHeader(SortComparer Comparer, bool Ascending, bool NullsFirst);
public sealed record SortRow(IReadOnlyList<object?> Keys, object? Value);

/// <summary>Represents normalized sort metadata and rows.</summary>
[ExpressifType(Name = "sort-table", Parent = "record", LiteralSyntax = "SortTable{headers := {...}, rows := {...}}", LiteralExamples = ["SortTable{headers := {}, rows := {}}"])]
public sealed class SortTableValue : RecordValue
{
    public SortTableValue(IEnumerable<SortHeader> headers, IEnumerable<SortRow> rows)
    {
        Headers = headers?.ToArray() ?? throw new ArgumentNullException(nameof(headers));
        Rows = rows?.ToArray() ?? throw new ArgumentNullException(nameof(rows));
        Set("headers", Headers.Select(header => new Tuple(header.Comparer, header.Ascending, header.NullsFirst)).ToArray());
        Set("rows", Rows.Select(row => new Tuple(new Tuple(row.Keys.ToArray()), row.Value)).ToArray());
    }

    public IReadOnlyList<SortHeader> Headers { get; }
    public IReadOnlyList<SortRow> Rows { get; }
}
