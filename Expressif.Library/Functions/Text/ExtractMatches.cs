using System.Text.RegularExpressions;

namespace Expressif.Functions.Text;

/// <summary>
/// Returns complete non-overlapping regular expression matches in source order, including zero-length matches. Returns an empty array for null or empty input or when no match is found.
/// </summary>
[Function(prefix: "")]
[Scope("text/selection")]
public class ExtractMatches : BaseTextFunction<string[]>
{
    public Func<string> Pattern { get; }

    /// <param name="pattern">The .NET regular expression identifying matches. Matching is case-sensitive unless inline options specify otherwise.</param>
    public ExtractMatches(Func<string> pattern)
        => Pattern = pattern;

    protected override object? EvaluateHighLevelString(string value)
        => value is "(null)" or "(empty)" or "(blank)"
            ? base.EvaluateHighLevelString(value)
            : EvaluateString(value);

    protected override object EvaluateNull() => System.Array.Empty<string>();
    protected override object EvaluateEmpty() => System.Array.Empty<string>();
    protected override object EvaluateBlank() => EvaluateString(" ");
    protected override object EvaluateString(string value)
        => value.Length == 0
            ? System.Array.Empty<string>()
            : new Regex(Pattern(), RegexOptions.None, TimeSpan.FromSeconds(1)).Matches(value).Select(match => match.Value).ToArray();
}
