using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Expressif.Values;

public class ContextObject
{
    public object? Value { get; private set; }
    private PropertyInfo[]? Cache { get; set; }

    public void Set(object value)
    {
        if (Value == null || Value.GetType() != value.GetType())
            Cache = null;
        Value = value;
    }

    public bool Exists(string name)
        => Value switch
        {
            DataRow row => row.Table.Columns.Contains(name),
            IDictionary dico => dico.Contains(name),
            IList => throw new NotNameableContextObjectException(Value),
            _ => RetrieveObjectProperty(name).Exists,
        };

    public object? this[string name]
    {
        get
        {
            return Value switch
            {
                DataRow row => row.Table.Columns.Contains(name) ? row[name] : throw new ArgumentOutOfRangeException(name),
                IDictionary dico => dico.Contains(name) ? dico[name] : throw new ArgumentOutOfRangeException(name),
                IList => throw new NotNameableContextObjectException(Value),
                _ => retrieveObjectProperty(name),
            };

            object? retrieveObjectProperty(string name)
            {
                var result = RetrieveObjectProperty(name);
                if (result.Exists)
                    return result.Value;
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }

    public bool Exists(int index)
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

    private (bool Exists, object? Value) RetrieveObjectProperty(string name)
    {
        if (Value == null)
            return (false, null);

        Cache ??= Value.GetType().GetProperties();

        var prop = Cache.FirstOrDefault(
            x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
            && x.CanRead
        );

        return (prop == null ? (false, null) : (true, prop.GetValue(Value)));
    }
}
