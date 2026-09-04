using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace Expressif.Functions.Text;

/// <summary>
/// Returns the argument value that has previously been HTML-encoded into a decoded string.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public class HtmlToText : BaseTextFunction
{
    protected override object EvaluateString(string value) => WebUtility.HtmlDecode(value);
}

/// <summary>
/// Returns the argument value converted to an HTML-encoded string
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public class TextToHtml : BaseTextFunction
{
    protected override object EvaluateString(string value) => WebUtility.HtmlEncode(value);
}

/// <summary>
/// Returns the input text escaped as URI data using UTF-8 percent encoding. Preserves `null`, empty, and blank inputs.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public sealed class TextToUri : BaseTextFunction
{
    protected override object EvaluateString(string value) => Uri.EscapeDataString(value);
}

/// <summary>
/// Returns text by unescaping one layer of URI percent encoding. Preserves `null`, empty, and blank inputs.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public sealed class UriToText : BaseTextFunction
{
    protected override object EvaluateString(string value) => Uri.UnescapeDataString(value);
}

/// <summary>
/// Returns the escaped contents of a JSON string without surrounding quotation marks. Preserves `null`, empty, and blank inputs.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public sealed class TextToJsonEscaped : BaseTextFunction
{
    protected override object? EvaluateString(string value)
    {
        var builder = new StringBuilder(value.Length);
        var remaining = value.AsSpan();
        while (!remaining.IsEmpty)
        {
            var status = Rune.DecodeFromUtf16(remaining, out var rune, out var charsConsumed);
            if (status != OperationStatus.Done)
                return null;

            switch (rune.Value)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                case <= 0x1F: builder.Append($"\\u{rune.Value:X4}"); break;
                default: builder.Append(rune.ToString()); break;
            }

            remaining = remaining[charsConsumed..];
        }

        return builder.ToString();
    }
}

/// <summary>
/// Returns text by decoding escaped JSON string contents without requiring surrounding quotation marks. Returns `null` for malformed input and preserves `null`, empty, and blank inputs.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public sealed class JsonEscapedToText : BaseTextFunction
{
    protected override object? EvaluateString(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<string>($"\"{value}\"");
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// Returns text escaped for use as XML character data without adding a containing element. Returns `null` for characters that are invalid in XML and preserves `null`, empty, and blank inputs.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public sealed class TextToXmlEscaped : BaseTextFunction
{
    protected override object? EvaluateString(string value)
    {
        try
        {
            XmlConvert.VerifyXmlChars(value);
            return value.Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal);
        }
        catch (XmlException)
        {
            return null;
        }
    }
}

/// <summary>
/// Returns text by decoding XML character data without requiring a containing element. Returns `null` for malformed input and preserves `null`, empty, and blank inputs.
/// </summary>
[Function(prefix: "")]
[Scope("text/encoding")]
public sealed class XmlEscapedToText : BaseTextFunction
{
    protected override object? EvaluateString(string value)
    {
        try
        {
            using var reader = XmlReader.Create(
                new StringReader($"<text>{value}</text>"),
                new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            reader.MoveToContent();
            return reader.ReadElementContentAsString();
        }
        catch (XmlException)
        {
            return null;
        }
    }
}
