using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Expressif.Values.Special;
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
            { typeof(object[]), () => new ArrayCaster().Cast(value) },
            { typeof(bool), () => new BooleanCaster().Cast(value) },
            { typeof(DateOnly), () => new DateOnlyCaster().Cast(value) },
            { typeof(DateTime), () => new DateTimeCaster().Cast(value) },
            { typeof(int), () => new IntegerCaster().Cast(value) },
            { typeof(decimal), () => new NumericCaster().Cast(value) },
            { typeof(string), () => new TextCaster().Cast(value) },
            { typeof(TimeOnly), () => new TimeOnlyCaster().Cast(value) },
            { typeof(YearMonth), () => new YearMonthCaster().Cast(value) },
        };

        if (@switch.TryGetValue(typeof(T), out var cast))
        {
            return (T?)cast.Invoke();
        }
        else
        {
            try
            { return ConvertTo<T>(value); }
            catch (Exception)
            { throw new ArgumentException($"Cannot convert the value '{value}' from a type '{value.GetType().Name}' to a '{typeof(T).Name}'"); }
        }
    }

    public virtual bool TryCast<T>(object? value, [NotNullWhen(true)] out T? result)
    {
        result = default;
        if (new Null().Equals(value) || new Empty().Equals(value) || new Whitespace().Equals(value))
            return false;

        if (value is T typed)
        {
            result = typed;
            return true;
        }

        if (!TryCast(value!, typeof(T), out var converted))
            return false;

        result = (T)converted!;
        return true;
    }

    private static bool TryCast(object value, Type targetType, out object? result)
    {
        result = null;
        if (targetType == typeof(bool) && new BooleanCaster().TryCast(value, out var boolean))
            result = boolean;
        else if (targetType == typeof(DateOnly) && new DateOnlyCaster().TryCast(value, out var date))
            result = date;
        else if (targetType == typeof(DateTime) && new DateTimeCaster().TryCast(value, out var dateTime))
            result = dateTime;
        else if (targetType == typeof(int) && new IntegerCaster().TryCast(value, out var integer))
            result = integer;
        else if (targetType == typeof(decimal) && new NumericCaster().TryCast(value, out var numeric))
            result = numeric;
        else if (targetType == typeof(string) && new TextCaster().TryCast(value, out var text))
            result = text;
        else if (targetType == typeof(TimeOnly) && new TimeOnlyCaster().TryCast(value, out var time))
            result = time;
        else
            return false;

        return true;
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
