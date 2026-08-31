using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values.Special;

namespace Expressif.Functions.Text;

/// <summary>
/// Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
/// </summary>
[Scope("text/tokenization")]
public class Token : BaseTextFunction
{
    public Func<int> Index { get; }
    public Func<char>? Separator { get; }

    /// <param name="index">An integer value between 0 and +Infinity, defining the position of the token to be returned.</param>
    public Token(Func<int> index)
        => (Index, Separator) = (index, null);

    /// <param name="index">An integer value between 0 and +Infinity, defining the position of the token to be returned.</param>
    /// <param name="separator">A character that delimits the substrings in this instance.</param>
    public Token(Func<int> index, Func<char> separator)
        => (Index, Separator) = (index, separator);
    protected override object EvaluateBlank() => Separator == null || char.IsWhiteSpace(Separator.Invoke()) ? new Null().Keyword : new Whitespace().Keyword;
    protected override object EvaluateEmpty() => new Null().Keyword;
    protected override object EvaluateString(string value)
    {
        var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Invoke());

        var tokens = tokenizer.Execute(value);
        var indexValue = Index.Invoke();
        if (indexValue < tokens.Length)
            return tokens[indexValue];
        else
            return new Null().Keyword;
    }
}

/// <summary>
/// Returns all tokens in the argument value in source order. By default, tokenization uses white-space characters as delimiters. If a character is specified, that character delimits the tokens.
/// </summary>
[Scope("text/tokenization")]
public class Tokenize : BaseTextFunction<string[]>
{
    public Func<char>? Separator { get; }

    /// <summary>
    /// Initializes tokenization with white-space delimiters.
    /// </summary>
    public Tokenize()
        => Separator = null;

    /// <param name="separator">A character that delimits the tokens in the argument value.</param>
    public Tokenize(Func<char> separator)
        => Separator = separator;

    protected override object EvaluateNull() => System.Array.Empty<string>();
    protected override object EvaluateEmpty() => System.Array.Empty<string>();
    protected override object EvaluateBlank() => System.Array.Empty<string>();

    protected override object EvaluateString(string value)
    {
        var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Invoke());
        return tokenizer.Execute(value);
    }
}

/// <summary>
/// Returns lexical tokens in source order, preserving punctuation and symbols as separate tokens.
/// </summary>
[Scope("text/tokenization")]
public class TokenizeLexical : BaseTextFunction<string[]>
{
    protected override object EvaluateNull() => System.Array.Empty<string>();
    protected override object EvaluateEmpty() => System.Array.Empty<string>();
    protected override object EvaluateBlank() => System.Array.Empty<string>();
    protected override object EvaluateString(string value) => new LexicalTokenizer().Execute(value);
}
