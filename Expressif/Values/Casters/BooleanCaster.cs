using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Expressif.Values.Casters;

public class BooleanCaster : BaseNumericCaster<bool>, ICaster<bool>, IParser<bool>
{
    protected override bool One { get => true; }

    protected override bool CastNumeric(object numeric)
        => Convert.ToBoolean(numeric, CultureInfo.InvariantCulture.NumberFormat);

    public bool Cast(object obj)
        => TryCast(obj, out var b)
        ? b
        : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type Boolean. The type Boolean can only be casted from the underlying numeric types (int, float, ...), and String. The expect string format can include decimal point, thousand separators, sign symbol and white spaces.");

    public override bool TryParse(string text, [NotNullWhen(true)] out bool value)
    {
        if (bool.TryParse(text, out var boolean))
            return (value = boolean) == value;

        return text.Trim().ToLowerInvariant() switch
        {
            "1" => (value = true) == value ,
            "yes" => (value = true) == value,
            "0" => (value = false) == value,
            "no" => (value = false) == value,
            _ => (value = false) != value,
        };
    }

    protected override bool TryNumericCast(object obj, [NotNullWhen(true)] out bool value)
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

    public virtual bool TryCastNumber<T>(T obj, [NotNullWhen(true)] out bool value)
        where T : INumber<T>
        => (value = obj != T.Zero) == value;
}
