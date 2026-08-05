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
        => Format(value, forRecordValue: false);

    private static string Format(object? value, bool forRecordValue)
    {
        if (value == null)
            return "null";

        if (TryFormatNamedCollection(value, out var named))
            return named;

        if (value is bool boolean)
            return boolean ? "true" : "false";

        if (value is string text)
            return forRecordValue ? RecordSyntax.FormatString(text) : text;

        if (value is DateOnly date)
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (value is IEnumerable enumerable && value is not string)
        {
            var values = new List<string>();
            foreach (var item in enumerable)
                values.Add(Format(item, forRecordValue));

            return $"{{{string.Join(", ", values)}}}";
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? "null";
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

        formatted = $"{{{string.Join(", ", fields.Select(x => $"{FormatFieldName(x.Key)} := {Format(x.Value, forRecordValue: true)}"))}}}";
        return true;
    }

    private static string FormatFieldName(string name)
        => RecordSyntax.IsBareToken(name)
            ? name
            : $"\"{RecordSyntax.EscapeDoubleQuoted(name)}\"";
}