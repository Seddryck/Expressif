using Expressif.Values;

namespace Expressif.Serialization;

/// <summary>Reads JSON as Expressif records, arrays, and scalar values.</summary>
public static class JsonValueReader
{
    /// <summary>Reads one JSON value. Nested values remain valid after parsing completes.</summary>
    public static object? Read(string text) => JsonValueParser.Read(text);

    /// <summary>Reads one JSON value without closing the caller's reader.</summary>
    public static object? Read(TextReader reader) => JsonValueParser.Read(reader);

    /// <summary>Reads one UTF-8 JSON value without closing the caller's stream.</summary>
    public static object? Read(Stream stream) => JsonValueParser.Read(stream);

    /// <summary>Reads source rows: a root array supplies its elements; any other root supplies one row.</summary>
    public static object?[] ReadRows(string text) => JsonValueParser.ReadRows(text);
}
