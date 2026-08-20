using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;

namespace Expressif.Values.Casters;

public abstract class BaseNumericCaster<T>
{
    protected readonly NumberStyles Style = NumberStyles.Number;
    protected readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;
    protected abstract T One { get; }
    public virtual bool TryCast(object obj, [NotNullWhen(true)] out T value)
    {
        try
        {
            if (TryNumericCast(obj, out value))
                return true;
        }
        catch (Exception exception) when (exception is OverflowException or FormatException or InvalidCastException)
        {
            value = default!;
            return false;
        }

        return obj switch
        {
            bool b => (value = (b ? One : default!)) is not null,
            string str => TryParse(str, out value),
            _ => (value = default!) is null
        };
    }

    protected abstract bool TryNumericCast(object obj, [NotNullWhen(true)] out T value);

    protected abstract T CastNumeric(object numeric);

    public abstract bool TryParse(string text, [NotNullWhen(true)] out T value);

    public virtual T Parse(string text)
        => TryParse(text, out var value) ? value : throw new FormatException();
}

public class NumericCaster : BaseNumericCaster<decimal>, ICaster<decimal>, IParser<decimal>
{
    protected override decimal One { get => 1m; }

    protected override bool TryNumericCast(object obj, [NotNullWhen(true)] out decimal value)
    {
        switch (obj)
        {
            case byte number: return TryCastNumber(number, out value);
            case sbyte number: return TryCastNumber(number, out value);
            case short number: return TryCastNumber(number, out value);
            case ushort number: return TryCastNumber(number, out value);
            case int number: return TryCastNumber(number, out value);
            case uint number: return TryCastNumber(number, out value);
            case long number: return TryCastNumber(number, out value);
            case ulong number: return TryCastNumber(number, out value);
            case float number: return TryCastNumber(number, out value);
            case double number: return TryCastNumber(number, out value);
            case decimal number: return TryCastNumber(number, out value);
            default:
                value = default;
                return false;
        }
    }

    public virtual bool TryCastNumber<T>(T obj, [NotNullWhen(true)] out decimal value)
        where T : INumber<T>
    {
        try
        {
            value = decimal.CreateChecked(obj);
            return true;
        }
        catch (OverflowException)
        {
            value = default;
            return false;
        }
    }

    protected override decimal CastNumeric(object numeric)
        => Convert.ToDecimal(numeric, CultureInfo.InvariantCulture.NumberFormat);

    public virtual decimal Cast(object obj)
        => TryCast(obj, out var d)
            ? d
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type Numeric. The type Numeric can only be casted from the underlying numeric types (int, float, ...), Boolean and String. The expect string format can include decimal point, thousand separators, sign symbol and white spaces.");

    public override bool TryParse(string text, [NotNullWhen(true)] out decimal value)
    {
        if (decimal.TryParse(text, Style, Format, out value))
            return true;
        if (string.Equals(text, "-INF", StringComparison.OrdinalIgnoreCase))
        {
            value = decimal.MinValue;
            return true;
        }
        if (string.Equals(text, "+INF", StringComparison.OrdinalIgnoreCase))
        {
            value = decimal.MaxValue;
            return true;
        }
        return false;
    }
}
