using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{

    /// <summary>
    /// Returns the argument value that has previously been HTML-encoded into a decoded string.
    /// </summary>
    [Function(prefix: "")]
    public class HtmlToText : BaseTextFunction
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlDecode(value);
    }

    /// <summary>
    /// Returns the argument value converted to an HTML-encoded string
    /// </summary>
    [Function(prefix: "")]
    public class TextToHtml : BaseTextFunction
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlEncode(value);
    }


}
