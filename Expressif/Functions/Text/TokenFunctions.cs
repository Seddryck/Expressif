using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{

    /// <summary>
    /// Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
    /// </summary>
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
    /// Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
    /// </summary>
    public class TokenCount : Length
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

}
