using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters;

public class Caster
{
    public virtual T? Cast<T>(object? value)
    {
        if (value == null || value == DBNull.Value)
            return default;

        if (value is T)
            return (T?)value;

        var @switch = new Dictionary<Type, Func<object>>
        {
            { typeof(bool), () => new BooleanCaster().Cast(value) },
            { typeof(DateTime), () => new DateTimeCaster().Cast(value) },
            { typeof(int), () => new IntegerCaster().Cast(value) },
            { typeof(decimal), () => new NumericCaster().Cast(value) },
            { typeof(string), () => new TextCaster().Cast(value) },
            { typeof(YearMonth), () => new YearMonthCaster().Cast(value) },
        };

        if (@switch.TryGetValue(typeof(T), out var cast))
            return (T?)cast.Invoke();
        else
            try
            { return ConvertTo<T>(value); }
            catch (Exception)
            { throw new ArgumentException($"Cannot convert the value '{value}' from a type '{value.GetType().Name}' to a '{typeof(T).Name}'"); }
    }

    protected virtual T? ConvertTo<T>(object value)
    {
        var targetType = typeof(T);

        if (value.GetType() == targetType)
            return (T)value;

        var converter = TypeDescriptor.GetConverter(value);
        if (converter?.CanConvertTo(targetType) ?? false)
            return (T?)converter.ConvertTo(value, targetType);

        converter = TypeDescriptor.GetConverter(targetType);
        if (converter?.CanConvertFrom(value.GetType()) ?? false)
            return (T?)converter.ConvertFrom(value);

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
