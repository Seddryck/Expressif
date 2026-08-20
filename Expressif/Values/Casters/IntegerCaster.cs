using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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

    public virtual bool TryCastNumber<T>(T obj, [NotNullWhen(true)] out int value)
        where T : INumber<T>
    {
        try
        {
            value = int.CreateChecked(obj);
            return T.CreateChecked(value) == obj;
        }
        catch (OverflowException)
        {
            value = default;
            return false;
        }
    }
}
