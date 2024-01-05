using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Expressif.Values;

public class ContextObject
{
    protected object? Value { get; private set; }
    private PropertyInfo[]? Cache { get; set; }

    public void Set(object value)
    {
        if (Value == null || Value.GetType() != value.GetType())
            Cache = null;
        Value = value;
    }

    public bool Contains(string name)
        => Value switch
        {
            DataRow row => row.Table.Columns.Contains(name),
            IDictionary dico => dico.Contains(name),
            IList => throw new NotNameableContextObjectException(Value),
            _ => TryRetrieveObjectProperty(name, out var _),
        };

    public object? this[string name]
    {
        get
        {
            return Value switch
            {
                DataRow row => row.Table.Columns.Contains(name) ? row[name] : throw new ArgumentOutOfRangeException(name),
                IDictionary dico =>  dico.Contains(name) ? dico[name] : throw new ArgumentOutOfRangeException(name),
                IList => throw new NotNameableContextObjectException(Value),
                _ => retrieveObjectProperty(name),
            };

            object? retrieveObjectProperty(string name)
                => TryRetrieveObjectProperty(name, out var value)
                    ? value
                    : throw new ArgumentOutOfRangeException(name);
        }
    }

    public bool TryGetValue(string name, out object? value)
    {
        var contains = Contains(name);
        value = contains ? this[name] : null;
        return contains;
    }

    public bool Contains(int index)
        => Value switch
        {
            DataRow row => index < row.Table.Columns.Count,
            IList list => index < list.Count,
            _ => throw new NotIndexableContextObjectException(Value)
        };

    public object? this[int index]
    {
        get
        {
            return Value switch
            {
                DataRow row => index < row.Table.Columns.Count ? row[index] : throw new ArgumentOutOfRangeException(index.ToString()),
                IList list => list[index],
                _ => throw new NotIndexableContextObjectException(Value)
            };
        }
    }

    public bool TryGetValue(int index, out object? value)
    {
        var contains = Contains(index);
        value = contains ? this[index] : null;
        return contains;
    }

    private bool TryRetrieveObjectProperty(string name, [NotNullWhen(true)] out object? value)
    {
        if (Value == null)
        {
            value = null;
            return false;
        }

        Cache ??= Value.GetType().GetProperties();

        var prop = Cache.FirstOrDefault(
            x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
            && x.CanRead
        );

        value = prop?.GetValue(Value);
        return prop != null;
    }
}
