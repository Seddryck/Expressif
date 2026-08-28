using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Expressif.Values;

public static class ValueFormatter
{
    public static string Format(object? value)
        => Format(value, structuredValue: false);

    private static string Format(object? value, bool structuredValue)
    {
        if (IsNullLike(value))
            return "null";

        if (TryFormatNamedCollection(value!, out var named))
            return named;

        if (value is TupleValue tuple)
            return $"T({string.Join(", ", tuple.Select(x => Format(x, structuredValue: true)))})";

        return FormatScalarOrEnumerable(value!, structuredValue);
    }

    private static bool IsNullLike(object? value)
        => value is null || value == DBNull.Value;

    private static string FormatScalarOrEnumerable(object value, bool structuredValue)
        => value switch
        {
            bool boolean => boolean ? "#true" : "#false",
            string text => structuredValue ? QuoteString(text) : text,
            DateOnly date => $"#{QuoteString(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}",
            DateTime dateTime => $"#{QuoteString(dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture))}",
            IEnumerable enumerable when value is not string => FormatEnumerable(enumerable),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? "null",
        };

    private static string QuoteString(string value)
        => $"\"{RecordSyntax.EscapeDoubleQuoted(value)}\"";

    private static string FormatEnumerable(IEnumerable enumerable)
    {
        var values = new List<string>();
        foreach (var item in enumerable)
            values.Add(Format(item, structuredValue: true));

        return $"{{{string.Join(", ", values)}}}";
    }

    private static bool TryFormatNamedCollection(object value, out string formatted)
    {
        IReadOnlyList<KeyValuePair<string, object?>>? fields = value switch
        {
            RecordValue record => record.ToArray(),
            IReadOnlyDictionary<string, object?> readOnly => readOnly.ToArray(),
            IDictionary<string, object?> dictionary => dictionary.ToArray(),
            IDictionary dictionary => dictionary.Cast<DictionaryEntry>()
                .Where(x => x.Key is not null)
                .Select(x => new KeyValuePair<string, object?>(x.Key!.ToString() ?? string.Empty, x.Value))
                .ToArray(),
            DataRow row => Enumerable.Range(0, row.Table.Columns.Count)
                .Select(i => new KeyValuePair<string, object?>(row.Table.Columns[i].ColumnName, row[i]))
                .ToArray(),
            ILiteDataRow row => Enumerable.Range(0, row.ColumnCount)
                .Select(i => new KeyValuePair<string, object?>(row.ColumnNames[i], row[i]))
                .ToArray(),
            _ => null,
        };

        if (fields is null)
        {
            formatted = string.Empty;
            return false;
        }

        formatted = $"{{{string.Join(", ", fields.Select(x => $"{FormatFieldName(x.Key)} := {Format(x.Value, structuredValue: true)}"))}}}";
        return true;
    }

    private static string FormatFieldName(string name)
        => RecordSyntax.IsBareToken(name)
            ? name
            : $"\"{RecordSyntax.EscapeDoubleQuoted(name)}\"";
}
