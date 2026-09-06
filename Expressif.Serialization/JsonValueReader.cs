using System.Text.Json;
using Expressif.Values;

namespace Expressif.Serialization;

/// <summary>Reads JSON as Expressif records, arrays, and scalar values.</summary>
public static class JsonValueReader
{
    /// <summary>Reads one JSON value. Nested values remain valid after parsing completes.</summary>
    public static object? Read(string text)
    {
        using var document = JsonDocument.Parse(text);
        return ConvertJsonValue(document.RootElement);
    }

    /// <summary>Reads one JSON value without closing the caller's reader.</summary>
    public static object? Read(TextReader reader) => Read(reader.ReadToEnd());

    /// <summary>Reads one UTF-8 JSON value without closing the caller's stream.</summary>
    public static object? Read(Stream stream)
    {
        using var document = JsonDocument.Parse(stream);
        return ConvertJsonValue(document.RootElement);
    }

    /// <summary>Reads source rows: a root array supplies its elements; any other root supplies one row.</summary>
    public static object?[] ReadRows(string text)
    {
        var value = Read(text);
        return value is object?[] rows ? rows : [value];
    }

    private static object? ConvertJsonValue(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonValue).ToArray(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => ConvertJsonNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new FormatException($"Unsupported JSON value kind '{element.ValueKind}'."),
        };

    private static RecordValue ConvertJsonObject(JsonElement element)
    {
        var record = new RecordValue();
        foreach (var property in element.EnumerateObject())
            record.Set(property.Name, ConvertJsonValue(property.Value));
        return record;
    }

    private static object ConvertJsonNumber(JsonElement element)
    {
        if (element.TryGetInt32(out var integer))
            return integer;
        if (element.TryGetDecimal(out var decimalNumber))
            return decimalNumber;
        return element.GetDouble();
    }
}
