namespace Expressif.Functions.Text;

/// <summary>
/// Returns the characters in the input text as an array in source order. Returns <see langword="null"/> for <see langword="null"/> and an empty array for empty text.
/// </summary>
[Function(prefix: "", aliases: ["chars"])]
[Scope("text/character")]
public class Chars : BaseTextFunction<string[]>
{
    protected override object? EvaluateHighLevelString(string value)
        => value is "(null)" or "(empty)" or "(blank)"
            ? base.EvaluateHighLevelString(value)
            : Split(value);

    protected override object? EvaluateNull()
        => null;

    protected override object EvaluateEmpty()
        => System.Array.Empty<string>();

    protected override object EvaluateBlank()
        => new[] { " " };

    protected override object EvaluateString(string value)
        => Split(value);

    private static string[] Split(string value)
        => value.Select(character => character.ToString()).ToArray();
}
