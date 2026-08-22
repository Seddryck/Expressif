using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters;

public class IntegerCaster : BaseNumericCaster<int>, ICaster<int>, IParser<int>
{
    protected override int One { get => 1; }

    protected override int CastNumeric(object numeric)
        => Convert.ToInt32(numeric, CultureInfo.InvariantCulture.NumberFormat);

    public virtual int Cast(object obj)
        => TryCast(obj, out var d)
            ? d
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type Integer. The type Integer can only be casted from the underlying numeric types (int, float, ...), Boolean and String. The expect string format can include decimal point, thousand separators, sign symbol and white spaces.");

    public override bool TryParse(string text, [NotNullWhen(true)] out int value)
        => int.TryParse(text, Style, Format, out value);

    protected override bool TryNumericCast(object obj, [NotNullWhen(true)] out int value)
    {
        switch (obj)
        {
            case byte number: return NumericCoercion.TryToInt(number, out value);
            case sbyte number: return NumericCoercion.TryToInt(number, out value);
            case short number: return NumericCoercion.TryToInt(number, out value);
            case ushort number: return NumericCoercion.TryToInt(number, out value);
            case int number: return NumericCoercion.TryToInt(number, out value);
            case uint number: return NumericCoercion.TryToInt(number, out value);
            case long number: return NumericCoercion.TryToInt(number, out value);
            case ulong number: return NumericCoercion.TryToInt(number, out value);
            case float number: return NumericCoercion.TryToInt(number, out value);
            case double number: return NumericCoercion.TryToInt(number, out value);
            case decimal number: return NumericCoercion.TryToInt(number, out value);
            default:
                value = default;
                return false;
        }
    }
}
