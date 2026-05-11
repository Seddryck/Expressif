using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Globalization;

namespace Expressif.Functions.Numeric;

[Function]
public abstract class FormatFunction : IFunction
{
    public object? Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull _ => EvaluateNull(),
            decimal numeric => EvaluateNumeric(numeric),
            _ => EvaluateUncasted(value),
        };
    }

    private object? EvaluateUncasted(object value)
    {
        if (new Null().Equals(value) || new Empty().Equals(value) || new Whitespace().Equals(value))
            return EvaluateNull();

        var caster = new NumericCaster();
        var numeric = caster.Cast(value);
        return EvaluateNumeric(numeric);
    }

    protected virtual object? EvaluateNull() => null;
    protected abstract string? EvaluateNumeric(decimal numeric);
}

internal static class HumanReadableFormatter
{
    internal static string FormatHumanReadable(decimal value, int precision, int @base, string[] units, bool omitDecimalsForBaseUnit)
    {
        var isNegative = value < 0;
        var scaled = Math.Abs(value);
        var unitIndex = 0;

        while (scaled >= @base && unitIndex < units.Length - 1)
        {
            scaled /= @base;
            unitIndex++;
        }

        var rounded = Math.Round(scaled, precision, MidpointRounding.AwayFromZero);
        if (rounded >= @base && unitIndex < units.Length - 1)
        {
            rounded /= @base;
            unitIndex++;
            rounded = Math.Round(rounded, precision, MidpointRounding.AwayFromZero);
        }

        string number;
        if (unitIndex == 0 && omitDecimalsForBaseUnit)
            number = Math.Round(rounded, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);
        else if (precision == 0)
            number = rounded.ToString("0", CultureInfo.InvariantCulture);
        else
            number = rounded.ToString($"F{precision}", CultureInfo.InvariantCulture);

        if (isNegative && number != "0")
            number = $"-{number}";

        var unit = units[unitIndex];
        return string.IsNullOrEmpty(unit) ? number : $"{number} {unit}";
    }
}

/// <summary>
/// Formats a numeric value using decimal SI prefixes.
/// </summary>
public class HumanReadableFormatDecimal : FormatFunction
{
    private static readonly string[] Units = ["", "k", "M", "G", "T", "P", "E"];
    public Func<int> Precision { get; }

    public HumanReadableFormatDecimal()
        : this(() => 2)
    { }

    public HumanReadableFormatDecimal(Func<int> precision)
        => Precision = precision;

    protected override string? EvaluateNumeric(decimal numeric)
    {
        var precision = Precision.Invoke();
        if (precision < 0 || precision > 3)
            return null;

        return HumanReadableFormatter.FormatHumanReadable(numeric, precision, 1000, Units, true);
    }
}

/// <summary>
/// Formats a numeric value as decimal bytes using SI prefixes.
/// </summary>
public class HumanReadableFormatDecimalBytes : FormatFunction
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
    public Func<int> Precision { get; }

    public HumanReadableFormatDecimalBytes()
        : this(() => 2)
    { }

    public HumanReadableFormatDecimalBytes(Func<int> precision)
        => Precision = precision;

    protected override string? EvaluateNumeric(decimal numeric)
    {
        var precision = Precision.Invoke();
        if (precision < 0 || precision > 3)
            return null;

        return HumanReadableFormatter.FormatHumanReadable(numeric, precision, 1000, Units, true);
    }
}

/// <summary>
/// Formats a numeric value as binary bytes using IEC prefixes.
/// </summary>
public class HumanReadableFormatBinaryBytes : FormatFunction
{
    private static readonly string[] Units = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];
    public Func<int> Precision { get; }

    public HumanReadableFormatBinaryBytes()
        : this(() => 2)
    { }

    public HumanReadableFormatBinaryBytes(Func<int> precision)
        => Precision = precision;

    protected override string? EvaluateNumeric(decimal numeric)
    {
        var precision = Precision.Invoke();
        if (precision < 0 || precision > 3)
            return null;

        return HumanReadableFormatter.FormatHumanReadable(numeric, precision, 1024, Units, true);
    }
}
