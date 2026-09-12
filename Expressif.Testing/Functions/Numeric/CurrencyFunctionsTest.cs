using System.Globalization;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class CurrencyFunctionsTest
{
    [Conformance]
    public void FormatCurrencyPrefix_Arity1(object? value, string symbol, string? expected)
        => Assert.That(new FormatCurrencyPrefix(() => symbol).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencyPrefix_Arity2(object? value, string symbol, int decimals, string? expected)
        => Assert.That(new FormatCurrencyPrefix(() => symbol, () => decimals).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencyPrefix_Arity3(object? value, string symbol, int decimals, string separator, string? expected)
        => Assert.That(new FormatCurrencyPrefix(() => symbol, () => decimals, () => separator).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencyPrefix_Arity4(object? value, string symbol, int decimals, string separator, string grouping, string? expected)
        => Assert.That(new FormatCurrencyPrefix(() => symbol, () => decimals, () => separator, () => grouping).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencyPrefix_Arity5(object? value, string symbol, int decimals, string separator, string grouping, string negative, string? expected)
        => Assert.That(new FormatCurrencyPrefix(() => symbol, () => decimals, () => separator, () => grouping, () => negative).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencySuffix_Arity1(object? value, string symbol, string? expected)
        => Assert.That(new FormatCurrencySuffix(() => symbol).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencySuffix_Arity2(object? value, string symbol, int decimals, string? expected)
        => Assert.That(new FormatCurrencySuffix(() => symbol, () => decimals).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencySuffix_Arity3(object? value, string symbol, int decimals, string separator, string? expected)
        => Assert.That(new FormatCurrencySuffix(() => symbol, () => decimals, () => separator).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencySuffix_Arity4(object? value, string symbol, int decimals, string separator, string grouping, string? expected)
        => Assert.That(new FormatCurrencySuffix(() => symbol, () => decimals, () => separator, () => grouping).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FormatCurrencySuffix_Arity5(object? value, string symbol, int decimals, string separator, string grouping, string negative, string? expected)
        => Assert.That(new FormatCurrencySuffix(() => symbol, () => decimals, () => separator, () => grouping, () => negative).Evaluate(value), Is.EqualTo(expected));

    [TestCase("123.4567 | numeric-to-format-currency-prefix(\"$\")", "$123.46")]
    [TestCase("57123.4567 | numeric-to-format-currency-suffix(\"€\", 0)", "57,123€")]
    [TestCase("57123.4567 | numeric-to-format-currency-suffix(\"EUR\", 2, \",\", \".\")", "57.123,46EUR")]
    [TestCase("-57123.4567 | numeric-to-format-currency-suffix(\"EUR\", 2, \",\", \".\", \"()\")", "(57.123,46EUR)")]
    [TestCase("1234.5 | format-currency-prefix(decimals := 0, symbol := \"$\")", "$1,235")]
    [TestCase("1234.5 | format-currency-suffix(symbol := \"$\")", "1,234.50$")]
    [TestCase("{price := 1234.5, symbol := \"EUR\"} | .price | format-currency-prefix(.symbol)", "EUR1,234.50")]
    public void Bind_Expression(string expression, string expected)
        => Assert.That(Expression.CreateClosed(expression).Evaluate(null), Is.EqualTo(expected));

    [TestCase("format-currency-prefix")]
    [TestCase("format-currency-suffix")]
    public void Introspection_ExposesContractAndParameters(string name)
    {
        var info = new FunctionIntrospector().Describe().Single(x => x.Name == name);
        Assert.Multiple(() =>
        {
            Assert.That(info.Input, Is.EqualTo("numeric"));
            Assert.That(info.Output, Is.EqualTo("text"));
            Assert.That(info.Aliases, Is.EqualTo(new[] { $"numeric-to-{name}" }));
            Assert.That(info.Parameters.Select(x => x.Name), Is.EqualTo(new[] { "symbol", "decimals", "separator", "grouping", "negative" }));
            Assert.That(info.Parameters.Select(x => x.Type), Is.EqualTo(new[] { "text", "integer", "text", "text", "text" }));
            Assert.That(info.Parameters.Select(x => x.Optional), Is.EqualTo(new[] { false, true, true, true, true }));
        });
    }

    [Test]
    public void Evaluate_TypedContract_IsIndependentOfCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            IFunction<decimal?, string?> function = new FormatCurrencyPrefix(() => "$");
            Assert.That(function.Evaluate(1234.565m), Is.EqualTo("$1,234.57"));
            Assert.That(function.Evaluate(decimal.MinValue), Is.EqualTo("-$79,228,162,514,264,337,593,543,950,335.00"));
            Assert.That(function.Evaluate(null), Is.Null);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Test]
    public void Evaluate_ArgumentsRunOnceInOrderOnlyForNumericInput()
    {
        var calls = new List<int>();
        var function = new FormatCurrencyPrefix(
            () => { calls.Add(1); return "$"; },
            () => { calls.Add(2); return 2; },
            () => { calls.Add(3); return "."; },
            () => { calls.Add(4); return ","; },
            () => { calls.Add(5); return "-"; });

        Assert.That(function.Evaluate((decimal?)null), Is.Null);
        Assert.That(calls, Is.Empty);
        Assert.That(function.Evaluate(10m), Is.EqualTo("$10.00"));
        Assert.That(calls, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }
}
