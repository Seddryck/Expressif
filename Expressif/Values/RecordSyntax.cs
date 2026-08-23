using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Expressif.Values;

public static class RecordSyntax
{
    private static readonly Regex BareTokenRegex = new(
        "^[A-Za-z0-9_+\\-]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100)
    );

    public static bool IsBareToken(string value)
        => value.Length > 0 && BareTokenRegex.IsMatch(value);

    public static bool TryParseTypedToken(string token, out object? value)
    {
        if (string.Equals(token, "null", StringComparison.OrdinalIgnoreCase))
        {
            value = null;
            return true;
        }

        if (string.Equals(token, "true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(token, "false", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
        {
            value = integer;
            return true;
        }

        if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric))
        {
            value = numeric;
            return true;
        }

        value = null;
        return false;
    }

    public static bool CanRenderBareString(string value)
        => IsBareToken(value) && !TryParseTypedToken(value, out _);

    public static string FormatString(string value)
        => CanRenderBareString(value)
            ? value
            : $"\"{EscapeDoubleQuoted(value)}\"";

    public static string EscapeDoubleQuoted(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);

    public static string UnescapeDoubleQuoted(string value)
    {
        var sb = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != '\\' || i == value.Length - 1)
            {
                sb.Append(value[i]);
                continue;
            }

            i++;
            sb.Append(value[i] switch
            {
                '"' => '"',
                '\\' => '\\',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                _ => value[i],
            });
        }

        return sb.ToString();
    }
}
