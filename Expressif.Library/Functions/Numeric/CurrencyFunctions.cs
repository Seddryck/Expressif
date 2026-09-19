using System;
using System.Globalization;

namespace Expressif.Functions.Numeric;

public abstract class CurrencyFunction : FormatFunction
{
    public Func<string> Symbol { get; }
    public Func<int> Decimals { get; }
    public Func<string> DecimalSeparator { get; }
    public Func<string> ThousandSeparator { get; }
    public Func<string> NegativeSymbol { get; }

    protected CurrencyFunction(Func<string> symbol, Func<int> decimals, Func<string> separator, Func<string> grouping, Func<string> negative)
    {
        Symbol = symbol;
        Decimals = decimals;
        DecimalSeparator = separator;
        ThousandSeparator = grouping;
        NegativeSymbol = negative;
    }

    protected abstract string PlaceSymbol(string number, string symbol);

    protected override string? EvaluateNumeric(decimal numeric)
    {
        var symbol = Symbol.Invoke();
        var decimals = Decimals.Invoke();
        var separator = DecimalSeparator.Invoke();
        var grouping = ThousandSeparator.Invoke();
        var negative = NegativeSymbol.Invoke();

        if (symbol is null || decimals < 0 || decimals > 28
            || string.IsNullOrEmpty(separator) || grouping is null
            || negative is null || negative.Length is < 1 or > 2)
            return null;

        var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        format.NumberDecimalSeparator = separator;
        format.NumberGroupSeparator = grouping;
        var magnitude = Math.Round(Math.Abs(numeric), decimals, MidpointRounding.AwayFromZero);
        var amount = PlaceSymbol(magnitude.ToString($"N{decimals}", format), symbol);
        return numeric >= 0
            ? amount
            : negative.Length == 1
                ? negative + amount
                : negative[0] + amount + negative[1];
    }
}

/// <summary>
/// Formats a numeric value as currency with the symbol before the number. Rounds midpoint values away from zero and returns null for null input or invalid formatting options.
/// </summary>
public class FormatCurrencyPrefix : CurrencyFunction
{
    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    public FormatCurrencyPrefix(Func<string> symbol)
        : this(symbol, () => 2) { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    public FormatCurrencyPrefix(Func<string> symbol, Func<int> decimals)
        : this(symbol, decimals, () => ".") { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    /// <param name="separator">Nonempty decimal separator; defaults to a point.</param>
    public FormatCurrencyPrefix(Func<string> symbol, Func<int> decimals, Func<string> separator)
        : this(symbol, decimals, separator, () => ",") { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    /// <param name="separator">Nonempty decimal separator; defaults to a point.</param>
    /// <param name="grouping">Thousands separator; defaults to a comma. An empty string disables grouping.</param>
    public FormatCurrencyPrefix(Func<string> symbol, Func<int> decimals, Func<string> separator, Func<string> grouping)
        : this(symbol, decimals, separator, grouping, () => "-") { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    /// <param name="separator">Nonempty decimal separator; defaults to a point.</param>
    /// <param name="grouping">Thousands separator; defaults to a comma. An empty string disables grouping.</param>
    /// <param name="negative">One character prepended to negative amounts, or two characters enclosing them; defaults to a minus sign.</param>
    public FormatCurrencyPrefix(Func<string> symbol, Func<int> decimals, Func<string> separator, Func<string> grouping, Func<string> negative)
        : base(symbol, decimals, separator, grouping, negative) { }

    protected override string PlaceSymbol(string number, string symbol)
        => symbol + number;
}

/// <summary>
/// Formats a numeric value as currency with the symbol after the number. Rounds midpoint values away from zero and returns null for null input or invalid formatting options.
/// </summary>
public class FormatCurrencySuffix : CurrencyFunction
{
    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    public FormatCurrencySuffix(Func<string> symbol)
        : this(symbol, () => 2) { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    public FormatCurrencySuffix(Func<string> symbol, Func<int> decimals)
        : this(symbol, decimals, () => ".") { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    /// <param name="separator">Nonempty decimal separator; defaults to a point.</param>
    public FormatCurrencySuffix(Func<string> symbol, Func<int> decimals, Func<string> separator)
        : this(symbol, decimals, separator, () => ",") { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    /// <param name="separator">Nonempty decimal separator; defaults to a point.</param>
    /// <param name="grouping">Thousands separator; defaults to a comma. An empty string disables grouping.</param>
    public FormatCurrencySuffix(Func<string> symbol, Func<int> decimals, Func<string> separator, Func<string> grouping)
        : this(symbol, decimals, separator, grouping, () => "-") { }

    /// <param name="symbol">Currency symbol placed next to the formatted number.</param>
    /// <param name="decimals">Number of decimal places from 0 to 28; defaults to 2.</param>
    /// <param name="separator">Nonempty decimal separator; defaults to a point.</param>
    /// <param name="grouping">Thousands separator; defaults to a comma. An empty string disables grouping.</param>
    /// <param name="negative">One character prepended to negative amounts, or two characters enclosing them; defaults to a minus sign.</param>
    public FormatCurrencySuffix(Func<string> symbol, Func<int> decimals, Func<string> separator, Func<string> grouping, Func<string> negative)
        : base(symbol, decimals, separator, grouping, negative) { }

    protected override string PlaceSymbol(string number, string symbol)
        => number + symbol;
}
