using System;
using System.Collections;
using System.Data;

namespace Expressif.Values;

public class ContextObject
{
    public object? Value { get; private set; }

    public void Set(object? value)
        => Value = value;

    public bool Contains(string name)
        => NamedValueAccessor.Contains(Value, name);

    public object? this[string name]
        => NamedValueAccessor.Get(Value, name);

    public bool TryGetValue(string name, out object? value)
        => NamedValueAccessor.TryGetValue(Value, name, out value);

    public bool Contains(int index)
        => Value switch
        {
            DataRow row => index < row.Table.Columns.Count,
            ILiteDataRow row => index < row.ColumnCount,
            TupleValue tuple => index >= 0 && index < tuple.Count,
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
                ILiteDataRow row => index < row.ColumnCount ? row[index] : throw new ArgumentOutOfRangeException(index.ToString()),
                TupleValue tuple => index >= 0 && index < tuple.Count ? tuple[index] : null,
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
}
