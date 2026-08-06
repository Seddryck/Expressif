using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Expressif.Values;

public static class NamedValueAccessor
{
    public static bool Contains(object? value, string name)
        => value switch
        {
            DataRow row => row.Table.Columns.Contains(name),
            IReadOnlyDictionary<string, object?> readOnly => readOnly.ContainsKey(name),
            ILiteDataRow row => row.ContainsColumn(name),
            IDictionary dico => dico.Contains(name),
            IList => throw new NotNameableContextObjectException(value),
            _ => TryRetrieveObjectProperty(value, name, out var _),
        };

    public static object? Get(object? value, string name)
    {
        return value switch
        {
            DataRow row => row.Table.Columns.Contains(name) ? row[name] : throw new ArgumentOutOfRangeException(name),
            IReadOnlyDictionary<string, object?> readOnly => readOnly.ContainsKey(name) ? readOnly[name] : throw new ArgumentOutOfRangeException(name),
            ILiteDataRow row => row.ContainsColumn(name) ? row[name] : throw new ArgumentOutOfRangeException(name),
            IDictionary dico => dico.Contains(name) ? dico[name] : throw new ArgumentOutOfRangeException(name),
            IList => throw new NotNameableContextObjectException(value),
            _ => RetrieveObjectProperty(value, name),
        };
    }

    public static bool TryGetValue(object? value, string name, out object? result)
    {
        var contains = Contains(value, name);
        result = contains ? Get(value, name) : null;
        return contains;
    }

    public static bool TryEnumerate(object? value, [NotNullWhen(true)] out IReadOnlyList<KeyValuePair<string, object?>>? values)
    {
        switch (value)
        {
            case null:
                values = null;
                return false;
            case DataRow row:
                var fromDataRow = new List<KeyValuePair<string, object?>>(row.Table.Columns.Count);
                for (var i = 0; i < row.Table.Columns.Count; i++)
                {
                    var column = row.Table.Columns[i];
                    fromDataRow.Add(new KeyValuePair<string, object?>(column.ColumnName, row[i]));
                }

                values = fromDataRow;
                return true;
            case ILiteDataRow row:
                var fromLiteRow = new List<KeyValuePair<string, object?>>(row.ColumnCount);
                for (var i = 0; i < row.ColumnCount; i++)
                    fromLiteRow.Add(new KeyValuePair<string, object?>(row.ColumnNames[i], row[i]));

                values = fromLiteRow;
                return true;
            case IReadOnlyDictionary<string, object?> readOnly:
                values = readOnly.ToArray();
                return true;
            case IDictionary<string, object?> genericDictionary:
                values = genericDictionary.ToArray();
                return true;
            case IDictionary dictionary:
                var fromDictionary = new List<KeyValuePair<string, object?>>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key == null)
                        continue;

                    fromDictionary.Add(new KeyValuePair<string, object?>(entry.Key.ToString() ?? string.Empty, entry.Value));
                }

                values = fromDictionary;
                return true;
            case string:
                values = null;
                return false;
            case IList:
                values = null;
                return false;
            default:
                var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                    .ToArray();

                values = properties
                    .Select(x => new KeyValuePair<string, object?>(x.Name, x.GetValue(value)))
                    .ToArray();

                return properties.Length > 0;
        }
    }

    private static object? RetrieveObjectProperty(object? value, string name)
        => TryRetrieveObjectProperty(value, name, out var result)
            ? result
            : throw new ArgumentException($"Cannot find a property named '{name}' in the object of type '{value?.GetType().Name ?? "null"}'.");

    private static bool TryRetrieveObjectProperty(object? value, string name, [NotNullWhen(true)] out object? result)
    {
        if (value == null)
        {
            result = null;
            return false;
        }

        var prop = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && x.CanRead);

        result = prop?.GetValue(value);
        return prop != null;
    }
}