using System.Numerics;
using System.Globalization;

namespace Expressif.Values.Casters;

public static class NumericCoercion
{
    private static readonly HashSet<Type> SupportedSourceTypeSet =
    [
        typeof(decimal),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(byte),
        typeof(uint),
        typeof(ulong),
        typeof(ushort),
        typeof(sbyte),
        typeof(float),
        typeof(double),
    ];

    public static IReadOnlySet<Type> SupportedSourceTypes => SupportedSourceTypeSet;

    public static bool IsSupported(Type sourceType)
        => SupportedSourceTypeSet.Contains(sourceType);

    public static bool TryToDecimal<T>(T value, out decimal result)
        where T : INumber<T>
    {
        if (!IsSupported(typeof(T)))
        {
            result = default;
            return false;
        }

        try
        {
            result = decimal.CreateChecked(value);
            return true;
        }
        catch (OverflowException)
        {
            result = default;
            return false;
        }
    }

    public static bool TryToDecimal(object? value, out decimal result)
    {
        switch (value)
        {
            case byte number: return TryToDecimal(number, out result);
            case sbyte number: return TryToDecimal(number, out result);
            case short number: return TryToDecimal(number, out result);
            case ushort number: return TryToDecimal(number, out result);
            case int number: return TryToDecimal(number, out result);
            case uint number: return TryToDecimal(number, out result);
            case long number: return TryToDecimal(number, out result);
            case ulong number: return TryToDecimal(number, out result);
            case float number: return TryToDecimal(number, out result);
            case double number: return TryToDecimal(number, out result);
            case decimal number: return TryToDecimal(number, out result);
            default: return new Caster().TryCast(value, out result);
        }
    }

    public static bool TryToInt<T>(T value, out int result)
        where T : INumber<T>
    {
        if (!IsSupported(typeof(T)))
        {
            result = default;
            return false;
        }

        try
        {
            result = int.CreateChecked(value);
            return T.CreateChecked(result) == value;
        }
        catch (OverflowException)
        {
            result = default;
            return false;
        }
    }

    public static bool TryToInt(object? value, out int result)
    {
        switch (value)
        {
            case byte number: return TryToInt(number, out result);
            case sbyte number: return TryToInt(number, out result);
            case short number: return TryToInt(number, out result);
            case ushort number: return TryToInt(number, out result);
            case int number: return TryToInt(number, out result);
            case uint number: return TryToInt(number, out result);
            case long number: return TryToInt(number, out result);
            case ulong number: return TryToInt(number, out result);
            case float number: return TryToInt(number, out result);
            case double number: return TryToInt(number, out result);
            case decimal number: return TryToInt(number, out result);
            default: return new Caster().TryCast(value, out result);
        }
    }

    public static bool TryToBoolean<T>(T value, out bool result)
        where T : INumber<T>
    {
        if (!IsSupported(typeof(T)))
        {
            result = default;
            return false;
        }

        result = value != T.Zero;
        return true;
    }

    public static bool TryToBoolean(object? value, out bool result)
    {
        switch (value)
        {
            case byte number: return TryToBoolean(number, out result);
            case sbyte number: return TryToBoolean(number, out result);
            case short number: return TryToBoolean(number, out result);
            case ushort number: return TryToBoolean(number, out result);
            case int number: return TryToBoolean(number, out result);
            case uint number: return TryToBoolean(number, out result);
            case long number: return TryToBoolean(number, out result);
            case ulong number: return TryToBoolean(number, out result);
            case float number: return TryToBoolean(number, out result);
            case double number: return TryToBoolean(number, out result);
            case decimal number: return TryToBoolean(number, out result);
            default: return new Caster().TryCast(value, out result);
        }
    }

    public static bool TryToText<T>(T value, out string? result)
        where T : INumber<T>
    {
        result = null;
        return TryToDecimal(value, out var numeric)
            && (result = numeric.ToString(CultureInfo.InvariantCulture.NumberFormat)) is not null;
    }

    public static bool TryToText(object? value, out string? result)
    {
        switch (value)
        {
            case byte number: return TryToText(number, out result);
            case sbyte number: return TryToText(number, out result);
            case short number: return TryToText(number, out result);
            case ushort number: return TryToText(number, out result);
            case int number: return TryToText(number, out result);
            case uint number: return TryToText(number, out result);
            case long number: return TryToText(number, out result);
            case ulong number: return TryToText(number, out result);
            case float number: return TryToText(number, out result);
            case double number: return TryToText(number, out result);
            case decimal number: return TryToText(number, out result);
            default: return new Caster().TryCast(value, out result);
        }
    }
}
