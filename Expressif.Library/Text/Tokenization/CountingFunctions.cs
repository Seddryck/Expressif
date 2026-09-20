using System;
using Expressif.Library.Text.Counting;

namespace Expressif.Library.Text.Tokenization;

/// <summary>
/// Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
/// </summary>
[Scope("text/tokenization")]
public class TokenCount : BaseTextCountingFunction
{
    public Func<char>? Separator { get; }
    public TokenCount()
        => Separator = null;

    /// <param name="separator">A character that delimits the substrings in this instance.</param>
    public TokenCount(Func<char> separator)
        => Separator = separator;

    protected override object? EvaluateBlank() => 0;
    protected override object? EvaluateString(string value) => CountToken(value);

    private int CountToken(string value)
    {
        var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Invoke());
        return tokenizer.Execute(value).Length;
    }
}

/// <summary>
/// Returns the number of lexical tokens in the argument value, including punctuation and symbols.
/// </summary>
[Scope("text/tokenization")]
public class TokenCountLexical : BaseTextCountingFunction
{
    protected override object? EvaluateBlank() => 0;
    protected override object? EvaluateString(string value) => new LexicalTokenizer().Execute(value).Length;
}
