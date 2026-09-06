# Expressif.Serialization

Converts JSON and CSV input into Expressif values without depending on the CLI.
The library targets .NET 8, 9, and 10. Expressif evaluation has no dependency on it.

```csharp
using Expressif.Serialization;

var value = JsonValueReader.Read("""{"name":"Ada","scores":[1,2,null]}""");
var rows = JsonValueReader.ReadRows("""[{"name":"Ada"},{"name":"Grace"}]""");

using var input = File.OpenRead("people.csv");
foreach (var row in CsvValueReader.Read(input, [new("delimiter", ";")]))
    Console.WriteLine(row);
```

`JsonValueReader.Read` accepts text, a `TextReader`, or a UTF-8 stream. Objects
become `RecordValue`, arrays become `object?[]`, and JSON null becomes null.
Numbers use `int`, then `decimal`, then `double`, in that order when representable.
Duplicate object keys keep the last value and the original field order.
`ReadRows` expands a root array into rows and wraps any other root in one row.
Malformed JSON throws `JsonException` (including its derived types).

`CsvValueReader.Read` lazily enumerates rows from the stream's current position.
It disposes its CSV reader when enumeration finishes, fails, or stops early, but
leaves the input stream open. Enumerations consume the stream and are not replayable
without repositioning it. `OpenDataReader` exposes the same conversion source to
hosts needing `IDataReader`; `leaveOpen: false` transfers stream ownership on success.
The caller must dispose the stream if opening fails. `SourceRows.Read` converts
data readers or enumerable sources into rows and disposes consumed readers.

CSV defaults match the CLI: comma delimiter, double quotes, CRLF line terminator,
and a required first header row. Headers must be nonempty and unique ignoring case.
Rows must have consistent widths. Use `new("line-terminator", "\n")` for LF input.
`new("header", false)` produces `column1`, `column2`, etc. Configured `header-rows`
or `header-repeat` lets the CSV parser consume headers and also uses generated
column names, preserving existing CLI behavior. Scalar mode requires one column.
CSV cells retain the existing profile's conversion rules, including null sequences;
CSV syntax, profile, header, and row-shape failures produce `FormatException`.

`CsvSourceOption` takes a name and an already parsed value. Hosts own argument
parsing. Text and character settings accept strings; booleans accept `bool`;
`header-rows` and `comment-rows` accept nonempty `object?[]` containing positive
one-based integers. `OriginalText` optionally preserves supplied text in diagnostics.
Options are applied in order, so the last occurrence wins.

Supported options: `delimiter`, `line-terminator`, `quote-char`, `double-quote`,
`escape-char`, `header`, `header-rows`, `header-join`, `header-repeat`, `comment-char`,
`comment-rows`, `null-sequence`, `missing-cell`, `skip-initial-space`,
`array-delimiter`, `array-prefix`, and `array-suffix`.
