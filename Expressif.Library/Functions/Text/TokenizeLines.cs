namespace Expressif.Functions.Text;

/// <summary>
/// Returns lines in source order, recognizing CR, LF, and CRLF as separators. Preserves spaces and empty lines, including a final empty line after a trailing separator. Returns an empty array for null or empty input.
/// </summary>
[Function(prefix: "")]
[Scope("text/tokenization")]
public class TokenizeLines : BaseTextFunction<string[]>
{
    protected override object? EvaluateHighLevelString(string value)
        => value is "(null)" or "(empty)" or "(blank)"
            ? base.EvaluateHighLevelString(value)
            : EvaluateString(value);

    protected override object EvaluateNull() => System.Array.Empty<string>();
    protected override object EvaluateEmpty() => System.Array.Empty<string>();
    protected override object EvaluateBlank() => new[] { " " };
    protected override object EvaluateString(string value)
        => value.Length == 0 ? System.Array.Empty<string>() : value.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
}
