using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters;

public class TextCaster : ICaster<string>, IParser<string>
{
    protected static readonly string DateFormat = "yyyy-MM-dd";
    protected static readonly string DateTimeFormat = $"{DateFormat} HH:mm:ss";
    protected static readonly NumberFormatInfo NumberFormat = CultureInfo.InvariantCulture.NumberFormat;

    public virtual bool TryCast(object obj, [NotNullWhen(true)] out string? value)
    {
        if (TypeChecker.IsNumericType(obj))
            return (value = Convert.ToDecimal(obj).ToString(NumberFormat)) == value;

        return obj switch
        {
            YearMonth yearMonth => (value = yearMonth.ToString()) is not null,
            DateTime dt => (value = dt.ToString(DateTimeFormat)) == value,
            DateOnly d => (value = d.ToString(DateFormat)) == value,
            string s => (value = s) == value,
            object o => (value = o.ToString()) == value,
            _ => (value = null) != value
        };
    }

    public virtual string Cast(object obj)
        => TryCast(obj, out var yearMonth)
            ? yearMonth
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type 'Text'. The type 'Text' can only be casted from types DateTime, DateOnly, Underlying numeric types, boolean and YearMonth.");

    public virtual bool TryParse(string text, [NotNullWhen(true)] out string? value)
        => (value = text) == value;

    public virtual string Parse(string text)
        => text;
}
