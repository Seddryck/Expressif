using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Expressif.Values;

public static class ValueFormatter
{
    public static string Format(object? value)
        => Format(value, ValueFormat.Compact);

    public static string Format(object? value, ValueFormat format)
    {
        if (!Enum.IsDefined(format))
            throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown value format.");

        var writer = new Writer(format);
        writer.Write(value, structuredValue: false);
        return writer.ToString();
    }

    private sealed class Writer(ValueFormat format)
    {
        private readonly StringBuilder builder = new();
        private readonly bool pretty = format == ValueFormat.Pretty;

        public void Write(object? value, bool structuredValue, int depth = 0)
        {
            if (IsNullLike(value))
            {
                builder.Append("null");
                return;
            }

            if (TryGetNamedCollection(value!, out var fields))
            {
                WriteNamedCollection(fields, depth);
                return;
            }

            switch (value)
            {
                case Grouping grouping:
                    WriteCollection("#{", "}", grouping, depth);
                    break;
                case DictionaryValue dictionary:
                    WriteCollection("!{", "}", dictionary, depth);
                    break;
                case PairValue pair:
                    WritePair(pair, depth);
                    break;
                case TupleValue tuple:
                    WriteCollection("T(", ")", tuple, depth);
                    break;
                default:
                    WriteScalarOrEnumerable(value!, structuredValue, depth);
                    break;
            }
        }

        public override string ToString() => builder.ToString();

        private void WriteScalarOrEnumerable(object value, bool structuredValue, int depth)
        {
            switch (value)
            {
                case bool boolean:
                    builder.Append(boolean ? "#true" : "#false");
                    break;
                case string text:
                    builder.Append(structuredValue ? QuoteString(text) : text);
                    break;
                case DateOnly date:
                    builder.Append('#').Append(QuoteString(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                    break;
                case DateTime dateTime:
                    builder.Append('#').Append(QuoteString(dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)));
                    break;
                case IEnumerable enumerable:
                    WriteCollection("{", "}", enumerable.Cast<object?>(), depth);
                    break;
                default:
                    builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? "null");
                    break;
            }
        }

        private void WriteCollection<T>(string opening, string closing, IEnumerable<T> values, int depth)
        {
            var items = values.Cast<object?>().ToArray();
            builder.Append(opening);
            if (items.Length == 0)
            {
                builder.Append(closing);
                return;
            }

            if (!pretty)
            {
                for (var index = 0; index < items.Length; index++)
                {
                    if (index > 0)
                        builder.Append(", ");
                    Write(items[index], structuredValue: true, depth);
                }
                builder.Append(closing);
                return;
            }

            builder.Append('\n');
            for (var index = 0; index < items.Length; index++)
            {
                WriteIndent(depth + 1);
                Write(items[index], structuredValue: true, depth + 1);
                if (index < items.Length - 1)
                    builder.Append(',');
                builder.Append('\n');
            }
            WriteIndent(depth);
            builder.Append(closing);
        }

        private void WritePair(PairValue pair, int depth)
        {
            builder.Append('(');
            if (!pretty)
            {
                Write(pair.Key, structuredValue: true, depth);
                builder.Append(" => ");
                Write(pair.Value, structuredValue: true, depth);
                builder.Append(')');
                return;
            }

            builder.Append('\n');
            WriteIndent(depth + 1);
            Write(pair.Key, structuredValue: true, depth + 1);
            builder.Append(" =>\n");
            WriteIndent(depth + 1);
            Write(pair.Value, structuredValue: true, depth + 1);
            builder.Append('\n');
            WriteIndent(depth);
            builder.Append(')');
        }

        private void WriteNamedCollection(IReadOnlyList<KeyValuePair<string, object?>> fields, int depth)
        {
            builder.Append('{');
            if (fields.Count == 0)
            {
                builder.Append('}');
                return;
            }

            if (pretty)
                builder.Append('\n');

            for (var index = 0; index < fields.Count; index++)
            {
                if (pretty)
                    WriteIndent(depth + 1);
                else if (index > 0)
                    builder.Append(", ");

                var field = fields[index];
                builder.Append(FormatFieldName(field.Key)).Append(" := ");
                Write(field.Value, structuredValue: true, pretty ? depth + 1 : depth);
                if (pretty)
                {
                    if (index < fields.Count - 1)
                        builder.Append(',');
                    builder.Append('\n');
                }
            }

            if (pretty)
                WriteIndent(depth);
            builder.Append('}');
        }

        private void WriteIndent(int depth) => builder.Append('\t', depth);
    }

    private static bool IsNullLike(object? value)
        => value is null || value == DBNull.Value;

    private static string QuoteString(string value)
        => $"\"{RecordSyntax.EscapeDoubleQuoted(value)}\"";

    private static bool TryGetNamedCollection(object value, out IReadOnlyList<KeyValuePair<string, object?>> fields)
    {
        fields = value switch
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
            _ => [],
        };

        return value is RecordValue
            or IReadOnlyDictionary<string, object?>
            or IDictionary<string, object?>
            or IDictionary
            or DataRow
            or ILiteDataRow;
    }

    private static string FormatFieldName(string name)
        => RecordSyntax.IsBareToken(name)
            ? name
            : $"\"{RecordSyntax.EscapeDoubleQuoted(name)}\"";
}
