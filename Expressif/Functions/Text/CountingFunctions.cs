using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text;

public abstract class BaseTextCountingFunction : BaseTextFunction
{
    protected override object EvaluateSpecial(string value) => -1;
    protected override object EvaluateBlank() => -1;
    protected override object EvaluateEmpty() => 0;
    protected override object EvaluateNull() => 0;
}

/// <summary>
/// Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. 
/// </summary>
[Function(prefix: "", aliases: new[] { "count-chars" })]
public class Length : BaseTextCountingFunction
{
    protected override object EvaluateSpecial(string value) => -1;
    protected override object EvaluateBlank() => -1;
    protected override object EvaluateEmpty() => 0;
    protected override object EvaluateNull() => 0;
    protected override object EvaluateString(string value) => value.Length;
}

/// <summary>
/// Returns the count of distinct chars in the textual argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. 
/// </summary>
public class CountDistinctChars : BaseTextCountingFunction
{
    protected override object EvaluateString(string value)
    {
        var chars = new List<char>(value.Length);
        for (int i = 0; i < value.Length; i++)
            if (!chars.Contains(value[i]))
                chars.Add(value[i]);
        return chars.Count;
    }
}

/// <summary>
/// Returns the count of non-overlapping occurrences of a substring, defined as a parameter, in the argument value.
/// </summary>
public class CountSubstring : BaseTextCountingFunction
{
    public Func<string> Substring { get; }

    /// <param name="substring">The substring to count in the argument value.</param>
    public CountSubstring(Func<string> substring)
        => Substring = substring;

    protected override object EvaluateString(string value)
    {
        var substring = Substring.Invoke();
        var index = 0;
        var count = 0;
        do
        {
            index = value.IndexOf(substring, index);
            if (index > -1)
            {
                count += 1;
                index += substring.Length;
            }
        }
        while (index != -1 && index <= value.Length - substring.Length);
        return count;
    }
}

/// <summary>
/// Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
/// </summary>
public class TokenCount : BaseTextCountingFunction
{
    public Func<char>? Separator { get; }
    public TokenCount()
        => Separator = null;

    /// <param name="separator">A character that delimits the substrings in this instance.</param>
    public TokenCount(Func<char> separator)
        => Separator = separator;

    protected override object EvaluateBlank() => 0;
    protected override object EvaluateString(string value) => CountToken(value);

    private int CountToken(string value)
    {
        var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Invoke());
        return tokenizer.Execute(value).Length;
    }
}
