using System.Text.RegularExpressions;

namespace Expressif.Functions.Text;

/// <summary>
/// Returns segments separated by regular expression matches in source order. Preserves spaces and empty segments without including captured delimiters. Returns an empty array for null or empty input.
/// </summary>
[Function(prefix: "")]
[Scope("text/tokenization")]
public class TokenizeRegex : BaseTextFunction<string[]>
{
    public Func<string> Pattern { get; }

    /// <param name="pattern">The .NET regular expression identifying delimiters. Matching is case-sensitive unless inline options specify otherwise.</param>
    public TokenizeRegex(Func<string> pattern)
        => Pattern = pattern;

    protected override object? EvaluateHighLevelString(string value)
        => value is "(null)" or "(empty)" or "(blank)"
            ? base.EvaluateHighLevelString(value)
            : EvaluateString(value);

    protected override object EvaluateNull() => System.Array.Empty<string>();
    protected override object EvaluateEmpty() => System.Array.Empty<string>();
    protected override object EvaluateBlank() => EvaluateString(" ");
    protected override object EvaluateString(string value)
    {
        if (value.Length == 0)
            return System.Array.Empty<string>();

        var regex = new Regex(Pattern(), RegexOptions.None, TimeSpan.FromSeconds(1));
        var segments = new List<string>();
        var start = 0;
        foreach (Match match in regex.Matches(value))
        {
            segments.Add(value[start..match.Index]);
            start = match.Index + match.Length;
        }
        segments.Add(value[start..]);
        return segments.ToArray();
    }
}
