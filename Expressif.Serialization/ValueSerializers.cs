using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Expressif.Values;

namespace Expressif.Serialization;

/// <summary>Resolves serializers for Expressif result values.</summary>
public static class ValueSerializers
{
    private static readonly IValueSerializer Raw = new RawValueSerializer();
    private static readonly IValueSerializer Json = new JsonValueSerializer();

    /// <summary>Resolves the serializer for a format.</summary>
    public static IValueSerializer Resolve(ValueSerializationFormat format)
        => format switch
        {
            ValueSerializationFormat.Raw => Raw,
            ValueSerializationFormat.Json => Json,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown value serialization format."),
        };

    private sealed class RawValueSerializer : IValueSerializer
    {
        public string Serialize(object? value, ValueFormat style = ValueFormat.Compact, string indentation = "  ")
            => ValueFormatter.Format(value, style, indentation);
    }

    private sealed class JsonValueSerializer : IValueSerializer
    {
        public string Serialize(object? value, ValueFormat style = ValueFormat.Compact, string indentation = "  ")
        {
            if (!Enum.IsDefined(style))
                throw new ArgumentOutOfRangeException(nameof(style), style, "Unknown value format.");
            ArgumentNullException.ThrowIfNull(indentation);
            if (indentation.Any(character => character is not ' ' and not '\t'))
                throw new ArgumentException("Indentation can contain only spaces and tabs.", nameof(indentation));

            var writer = new JsonWriter(style == ValueFormat.Pretty, indentation);
            writer.Write(value);
            return writer.ToString();
        }
    }

    private sealed class JsonWriter(bool pretty, string indentation)
    {
        private readonly StringBuilder builder = new();

        public void Write(object? value, int depth = 0)
        {
            if (value is null || value == DBNull.Value)
            {
                builder.Append("null");
                return;
            }

            if (TryGetNamedCollection(value, out var fields))
            {
                WriteObject(fields, depth);
                return;
            }

            switch (value)
            {
                case bool boolean:
                    builder.Append(boolean ? "true" : "false");
                    break;
                case string text:
                    WriteString(text);
                    break;
                case char character:
                    WriteString(character.ToString());
                    break;
                case DateOnly date:
                    WriteString(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    break;
                case DateTime dateTime:
                    WriteString(dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture));
                    break;
                case DateTimeOffset dateTimeOffset:
                    WriteString(dateTimeOffset.ToString("O", CultureInfo.InvariantCulture));
                    break;
                case TimeOnly time:
                    WriteString(time.ToString("O", CultureInfo.InvariantCulture));
                    break;
                case Guid guid:
                    WriteString(guid.ToString());
                    break;
                case PairValue pair:
                    WriteArray([pair.Key, pair.Value], depth);
                    break;
                case IEnumerable enumerable:
                    WriteArray(enumerable.Cast<object?>(), depth);
                    break;
                default:
                    WriteScalar(value);
                    break;
            }
        }

        public override string ToString() => builder.ToString();

        private void WriteObject(IReadOnlyList<KeyValuePair<string, object?>> fields, int depth)
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

                WriteString(fields[index].Key);
                builder.Append(pretty ? ": " : ":");
                Write(fields[index].Value, depth + 1);
                WriteSeparator(index, fields.Count);
            }
            if (pretty)
                WriteIndent(depth);
            builder.Append('}');
        }

        private void WriteArray(IEnumerable<object?> values, int depth)
        {
            var items = values.ToArray();
            builder.Append('[');
            if (items.Length == 0)
            {
                builder.Append(']');
                return;
            }

            if (pretty)
                builder.Append('\n');
            for (var index = 0; index < items.Length; index++)
            {
                if (pretty)
                    WriteIndent(depth + 1);
                Write(items[index], depth + 1);
                WriteSeparator(index, items.Length);
            }
            if (pretty)
                WriteIndent(depth);
            builder.Append(']');
        }

        private void WriteSeparator(int index, int count)
        {
            if (index < count - 1)
                builder.Append(',');
            if (pretty)
                builder.Append('\n');
        }

        private void WriteIndent(int depth)
        {
            for (var index = 0; index < depth; index++)
                builder.Append(indentation);
        }

        private void WriteString(string value) => builder.Append(JsonSerializer.Serialize(value));

        private void WriteScalar(object value)
        {
            if (value.GetType().IsEnum)
            {
                WriteString(value.ToString()!);
                return;
            }

            builder.Append(JsonSerializer.Serialize(value, value.GetType()));
        }

        private static bool TryGetNamedCollection(object value, out IReadOnlyList<KeyValuePair<string, object?>> fields)
        {
            fields = value switch
            {
                RecordValue record => record.ToArray(),
                IReadOnlyDictionary<string, object?> readOnly => readOnly.ToArray(),
                IDictionary<string, object?> dictionary => dictionary.ToArray(),
                IDictionary dictionary => dictionary.Cast<DictionaryEntry>()
                    .Where(entry => entry.Key is not null)
                    .Select(entry => new KeyValuePair<string, object?>(entry.Key!.ToString() ?? string.Empty, entry.Value))
                    .ToArray(),
                DataRow row => Enumerable.Range(0, row.Table.Columns.Count)
                    .Select(index => new KeyValuePair<string, object?>(row.Table.Columns[index].ColumnName, row[index]))
                    .ToArray(),
                ILiteDataRow row => Enumerable.Range(0, row.ColumnCount)
                    .Select(index => new KeyValuePair<string, object?>(row.ColumnNames[index], row[index]))
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
    }
}
