using System.Globalization;
using Expressif.Bindings;
using Expressif.Syntax;
using Expressif.Values;

namespace Expressif.Cli.Inputs;

internal interface IInputValueParser
{
    object? Parse(string text);
    object? ParseStrict(string text);
}

internal sealed class CliInputValueParser : IInputValueParser
{
    private readonly ParameterValueConverter converter = new();

    public object? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        try { return converter.Parse(text); }
        catch (Exception exception) when (exception is ExpressifSyntaxException or BindingException)
        {
            if (TryParseAutoQuotedScalar(text, out var value))
                return value;
            throw new FormatException(exception.Message, exception);
        }
    }

    public object? ParseStrict(string text)
    {
        try { return converter.Parse(text); }
        catch (Exception exception) when (exception is ExpressifSyntaxException or BindingException)
        { throw new FormatException(exception.Message, exception); }
    }

    private bool TryParseAutoQuotedScalar(string text, out object? value)
    {
        value = null;
        var normalized = text.Trim();
        if (normalized.Length == 0 || normalized.StartsWith('{') || normalized.StartsWith("T(", StringComparison.Ordinal)
            || normalized.IndexOfAny(['{', '}', '[', ']', '@', '#', '|', ',', ':']) >= 0)
            return false;

        if (DateOnly.TryParseExact(normalized, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        { value = date; return true; }
        if (RecordSyntax.TryParseTypedToken(normalized, out var typedValue) && typedValue is not null and not bool)
        { value = typedValue; return true; }

        var candidate = normalized.Contains('`')
            ? $"\"{RecordSyntax.EscapeDoubleQuoted(normalized)}\""
            : $"`{normalized}`";
        try { value = converter.Parse(candidate); return true; }
        catch (Exception exception) when (exception is ExpressifSyntaxException or BindingException)
        { return false; }
    }
}
