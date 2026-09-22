using System;
using System.Linq;
using System.Text;
using Expressif.Library.Text;
using Expressif.Values.Special;

namespace Expressif.Library.Text.Normalization;

/// <summary>
/// returns the argument with any two or more consecutive whitespaces replaced by the first whitespace in the sequence and trimming the result. `\r\n` is considered as a single character.
/// </summary>
[Scope("text/normalization")]
public class CollapseWhitespace : BaseTextFunction
{
    protected override object? EvaluateString(string value)
    {
        char? previousWithespace = null;
        var stringBuilder = new StringBuilder();
        foreach (var c in value)
        {
            if (previousWithespace == '\r' && c == '\n')
                previousWithespace = null;
            if (previousWithespace is null || !char.IsWhiteSpace(c))
                stringBuilder.Append(c);
            previousWithespace = char.IsWhiteSpace(c) ? c : null;
        }
        return stringBuilder.ToString().Trim();
    }

    protected override object? EvaluateBlank()
        => Expressif.Values.Special.Empty.Instance.Keyword;
}

/// <summary>
/// returns the argument with any whitespace replaced by a space character. `\r\n` is considered as a single character.
/// </summary>
[Scope("text/normalization")]
public class CleanWhitespace : BaseTextFunction
{
    protected override object? EvaluateString(string value)
    {
        char? previousWithespace = null;
        var stringBuilder = new StringBuilder();
        foreach (var c in value)
        {
            if (!(previousWithespace == '\r' && c == '\n'))
                stringBuilder.Append(char.IsWhiteSpace(c) ? ' ' : c);
            previousWithespace = char.IsWhiteSpace(c) ? c : null;
        }

        return stringBuilder.ToString();
    }
    protected override object? EvaluateBlank()
        => Whitespace.Instance.Keyword;
}
