using Expressif.Functions.Numeric;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Expressif.Functions.Text
{
    [Function]
    abstract class BaseTextFunction : IFunction
    {
        public object? Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                Null => EvaluateNull(),
                DBNull _ => EvaluateNull(),
                Empty _ => EvaluateEmpty(),
                Whitespace _ => EvaluateBlank(),
                string s => EvaluateHighLevelString(s),
                _ => EvaluateUncasted(value),
            };
        }

        private object EvaluateUncasted(object value)
        {
            var caster = new TextCaster();
            var str = caster.Execute(value);
            return EvaluateHighLevelString(str);
        }

        protected virtual object EvaluateHighLevelString(string value)
        {
            if (new Empty().Equals(value))
                return EvaluateEmpty();

            if (new Null().Equals(value))
                return EvaluateNull();


            if (new Whitespace().Equals(value))
                return EvaluateBlank();

            if (value.StartsWith("(") && value.EndsWith(")"))
                return EvaluateSpecial(value);

            return EvaluateString(value);
        }

        protected virtual object EvaluateNull() => new Null().Keyword;
        protected virtual object EvaluateEmpty() => new Empty().Keyword;
        protected virtual object EvaluateBlank() => new Whitespace().Keyword;
        protected virtual object EvaluateSpecial(string value) => value;
        protected abstract object EvaluateString(string value);
    }

    /// <summary>
    /// Returns the argument value converted to an HTML-encoded string
    /// </summary>
    [Function(prefix: "")]
    class TextToHtml : BaseTextFunction
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlEncode(value);
    }

    abstract class BaseTextCasing : BaseTextFunction
    { }

    /// <summary>
    /// Returns the argument value converted to lowercase using the casing rules of the invariant culture.
    /// </summary>
    class Lower : BaseTextCasing
    {
        protected override object EvaluateString(string value) => value.ToLowerInvariant();
    }

    /// <summary>
    /// Returns the argument value converted to uppercase using the casing rules of the invariant culture.
    /// </summary>
    class Upper : BaseTextCasing
    {
        protected override object EvaluateString(string value) => value.ToUpperInvariant();
    }

    /// <summary>
    /// Returns the argument value without all leading or trailing white-space characters.
    /// </summary>
    class Trim : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value.Trim();
    }

    abstract class BaseTextAppend : BaseTextFunction
    {
        public IScalarResolver<string> Append { get; }
        public BaseTextAppend(IScalarResolver<string> append)
            => Append = append;

        protected override object EvaluateEmpty() => Append.Execute() ?? string.Empty;
        protected override object EvaluateBlank() => Append.Execute() ?? string.Empty;
    }

    /// <summary>
    /// Returns the argument value preceeded by the parameter value.
    /// </summary>
    class Prefix : BaseTextAppend
    {
        /// <param name="prefix">The text to append</param>
        public Prefix(IScalarResolver<string> prefix)
            : base(prefix) { }
        protected override object EvaluateString(string value) => $"{Append.Execute()}{value}";
    }

    /// <summary>
    /// Returns the argument value followed by the parameter value.
    /// </summary>
    class Suffix : BaseTextAppend
    {
        /// <param name="suffix">The text to append</param>
        public Suffix(IScalarResolver<string> suffix)
            : base(suffix) { }
        protected override object EvaluateString(string value) => $"{value}{Append.Execute()}";
    }

    abstract class BaseTextLength : BaseTextFunction
    {
        public IScalarResolver<int> Length { get; }

        public BaseTextLength(IScalarResolver<int> length)
            => Length = length;
    }

    /// <summary>
    /// Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.
    /// </summary>
    class FirstChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to return</param>
        public FirstChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value.Substring(0, Length.Execute()) : value;
    }

    /// <summary>
    /// Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.
    /// </summary>
    class LastChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to return</param>
        public LastChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value.Substring(value.Length - Length.Execute(), Length.Execute()) : value;
    }

    /// <summary>
    /// Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. 
    /// </summary>
    class SkipFirstChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to skip</param>
        public SkipFirstChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length <= Length.Execute() ? new Empty().Keyword : value.Substring(Length.Execute(), value.Length - Length.Execute());
    }

    /// <summary>
    /// Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. 
    /// </summary>
    class SkipLastChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to skip</param>
        public SkipLastChars(IScalarResolver<int> length) 
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length <= Length.Execute() ? new Empty().Keyword : value.Substring(0, value.Length - Length.Execute());
    }

    abstract class BasePaddingFunction : BaseTextLength
    {
        public IScalarResolver<char> Character { get; }

        public BasePaddingFunction(IScalarResolver<int> length, IScalarResolver<char> character)
            : base(length)
            => Character = character;

        protected override object EvaluateEmpty() => new string(Character.Execute(), Length.Execute());
        protected override object EvaluateNull() => new string(Character.Execute(), Length.Execute());

    }

    /// <summary>
    /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. 
    /// </summary>
    class PadRight : BasePaddingFunction
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the minimal length of the string returned</param>
        /// <param name="character">The padding character</param>
        public PadRight(IScalarResolver<int> length, IScalarResolver<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value : value.PadRight(Length.Execute(), Character.Execute());
    }

    /// <summary>
    /// Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. 
    /// </summary>
    class PadLeft : BasePaddingFunction
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the minimal length of the string returned</param>
        /// <param name="character">The padding character</param>
        public PadLeft(IScalarResolver<int> length, IScalarResolver<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value : value.PadLeft(Length.Execute(), Character.Execute());
    }

    /// <summary>
    /// Returns the argument value except if this value only contains white-space characters then it returns `empty`.
    /// </summary>
    [Function(prefix:"", aliases: new[] {"blank-to-empty"})]
    class WhitespacesToEmpty : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value only contains white-space characters then it returns `null`.
    /// </summary>
    [Function(prefix: "", aliases: new[] { "blank-to-null" })]
    class WhitespacesToNull : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Null().Keyword;
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value is `empty` then it returns `null`.
    /// </summary>
    [Function(prefix: "")]
    class EmptyToNull : BaseTextFunction
    {
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value is `null` then it returns `empty`.
    /// </summary>
    [Function(prefix: "")]
    class NullToEmpty : BaseTextFunction
    {
        protected override object EvaluateNull() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value that has previously been HTML-encoded into a decoded string.
    /// </summary>
    [Function(prefix: "")]
    class HtmlToText : BaseTextFunction
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlDecode(value);
    }

    /// <summary>
    /// Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. 
    /// </summary>
    class Length : BaseTextFunction
    {
        protected override object EvaluateSpecial(string value) => -1;
        protected override object EvaluateBlank() => -1;
        protected override object EvaluateEmpty() => 0;
        protected override object EvaluateNull() => 0;
        protected override object EvaluateString(string value) => value.Length;
    }

    /// <summary>
    /// Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
    /// </summary>
    class Token : BaseTextFunction
    {
        public IScalarResolver<int> Index { get; }
        public IScalarResolver<char>? Separator { get; }

        /// <param name="index">An integer value between 0 and +Infinity, defining the position of the token to be returned.</param>
        public Token(IScalarResolver<int> index)
            => (Index, Separator) = (index, null);

        /// <param name="index">An integer value between 0 and +Infinity, defining the position of the token to be returned.</param>
        /// <param name="separator">A character that delimits the substrings in this instance.</param>
        public Token(IScalarResolver<int> index, IScalarResolver<char> separator)
            => (Index, Separator) = (index, separator);
        protected override object EvaluateBlank() => Separator == null || char.IsWhiteSpace(Separator.Execute()) ? new Null().Keyword : new Whitespace().Keyword;
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value)
        {
            var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Execute());

            var tokens = tokenizer.Execute(value);
            var indexValue = Index.Execute();
            if (indexValue < tokens.Length)
                return tokens[indexValue];
            else
                return new Null().Keyword;
        }
    }

    /// <summary>
    /// Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.
    /// </summary>
    class TokenCount : Length
    {
        public IScalarResolver<char>? Separator { get; }
        public TokenCount()
            => Separator = null;
        
        /// <param name="separator">A character that delimits the substrings in this instance.</param>
        public TokenCount(IScalarResolver<char> separator)
            => Separator = separator;

        protected override object EvaluateBlank() => 0;
        protected override object EvaluateString(string value) => CountToken(value);

        private int CountToken(string value)
        {
            var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Execute());
            return tokenizer.Execute(value).Length;
        }
    }

    /// <summary>
    /// Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter.
    /// </summary>
    [Function(prefix: "")]
    class TextToDateTime : BaseTextFunction
    {
        public IScalarResolver<string> Format { get; }
        public IScalarResolver<string> Culture { get; }

        /// <param name="format">A string representing the required format.</param>
        public TextToDateTime(IScalarResolver<string> format)
            => (Format, Culture) = (format, new LiteralScalarResolver<string>(string.Empty));

        /// <param name="format">A string representing the required format.</param>
        /// <param name="culture">A string representing a pre-defined culture.</param>
        public TextToDateTime(IScalarResolver<string> format, IScalarResolver<string> culture)
            => (Format, Culture) = (format, culture);

        protected override object EvaluateString(string value)
        {
            var info = (string.IsNullOrEmpty(Culture.Execute()) ? CultureInfo.InvariantCulture : new CultureInfo(Culture.Execute()!)).DateTimeFormat;

            if (DateTime.TryParseExact(value, Format.Execute(), info, DateTimeStyles.RoundtripKind, out var dateTime))
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

            throw new ArgumentException($"Impossible to transform the value '{value}' into a date using the format '{Format}'");
        }
    }

    /// <summary>
    /// Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.
    /// </summary>
    class RemoveChars : BaseTextFunction
    {
        public IScalarResolver<char> CharToRemove { get; }

        /// <param name="charToRemove">The char to be removed from the argument string</param>
        public RemoveChars(IScalarResolver<char> charToRemove)
            => CharToRemove = charToRemove;

        protected override object EvaluateString(string value)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in value)
                if (!c.Equals(CharToRemove.Execute()))
                    stringBuilder.Append(c);
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
        {
            if (char.IsWhiteSpace(CharToRemove.Execute()))
                return new Empty().Keyword;
            else
                return base.EvaluateBlank();
        }
    }

    /// <summary>
    /// Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced.
    /// </summary>
    [Function(prefix: "")]
    class TextToMask : BaseTextFunction
    {
        private char maskChar { get; } = '*';
        public IScalarResolver<string> Mask { get; }

        /// <param name="mask">The string representing the mask to apply to the argument string</param>
        public TextToMask(IScalarResolver<string> mask)
            => Mask = mask;

        protected override object EvaluateString(string value)
        {
            var mask = Mask.Execute() ?? string.Empty;
            var stringBuilder = new StringBuilder();
            var index = 0;
            foreach (var c in mask)
                if (c.Equals(maskChar))
                    stringBuilder.Append(index < value.Length ? value[index++] : maskChar);
                else
                    stringBuilder.Append(c);
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
            => Mask.Execute() ?? string.Empty;
        protected override object EvaluateEmpty()
            => Mask.Execute() ?? string.Empty;
    }

    /// <summary>
    /// Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.
    /// </summary>
    [Function(prefix: "")]
    class MaskToText : BaseTextFunction
    {
        private char maskChar { get; } = '*';
        public IScalarResolver<string> Mask { get; }

        /// <param name="mask">The string representing the mask to be unset from the argument string</param>
        public MaskToText(IScalarResolver<string> mask)
            => Mask = mask;

        protected override object EvaluateString(string value)
        {
            var mask = Mask.Execute() ?? string.Empty;
            var stringBuilder = new StringBuilder();
            if (mask.Length != value.Length)
                return new Null().Keyword;

            for (int i = 0; i < mask.Length; i++)
                if (mask[i].Equals(maskChar) && !value[i].Equals(maskChar))
                    stringBuilder.Append(value[i]);
                else if (!mask[i].Equals(value[i]))
                    return new Null().Keyword;
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
            => ((Mask.Execute() ?? string.Empty).Replace(maskChar.ToString(), "").Length == 0) ? new Whitespace().Keyword : new Null().Keyword;
        protected override object EvaluateEmpty()
            => ((Mask.Execute() ?? string.Empty).Replace(maskChar.ToString(), "").Length == 0) ? new Empty().Keyword : new Null().Keyword;
    }
}
