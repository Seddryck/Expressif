using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
