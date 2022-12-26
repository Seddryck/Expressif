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
    public abstract class BaseTextFunction : IFunction
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
    public class TextToHtml : BaseTextFunction
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlEncode(value);
    }

    public abstract class BaseTextCasing : BaseTextFunction
    { }

    /// <summary>
    /// Returns the argument value converted to lowercase using the casing rules of the invariant culture.
    /// </summary>
    public class Lower : BaseTextCasing
    {
        protected override object EvaluateString(string value) => value.ToLowerInvariant();
    }

    /// <summary>
    /// Returns the argument value converted to uppercase using the casing rules of the invariant culture.
    /// </summary>
    public class Upper : BaseTextCasing
    {
        protected override object EvaluateString(string value) => value.ToUpperInvariant();
    }

    /// <summary>
    /// Returns the argument value without all leading or trailing white-space characters.
    /// </summary>
    public class Trim : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value.Trim();
    }

    public abstract class BaseTextAppend : BaseTextFunction
    {
        public Func<string> Append { get; }
        public BaseTextAppend(Func<string> append)
            => Append = append;

        protected override object EvaluateEmpty() => Append.Invoke() ?? string.Empty;
        protected override object EvaluateBlank() => Append.Invoke() ?? string.Empty;
    }

    /// <summary>
    /// Returns the argument value preceeded by the parameter value.
    /// </summary>
    public class Prefix : BaseTextAppend
    {
        /// <param name="prefix">The text to append</param>
        public Prefix(Func<string> prefix)
            : base(prefix) { }
        protected override object EvaluateString(string value) => $"{Append.Invoke()}{value}";
    }

    /// <summary>
    /// Returns the argument value followed by the parameter value.
    /// </summary>
    public class Suffix : BaseTextAppend
    {
        /// <param name="suffix">The text to append</param>
        public Suffix(Func<string> suffix)
            : base(suffix) { }
        protected override object EvaluateString(string value) => $"{value}{Append.Invoke()}";
    }

    public abstract class BaseTextLength : BaseTextFunction
    {
        public Func<int> Length { get; }

        public BaseTextLength(Func<int> length)
            => Length = length;
    }

    /// <summary>
    /// Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.
    /// </summary>
    public class FirstChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to return</param>
        public FirstChars(Func<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Invoke() ? value[..Length.Invoke()] : value;
    }

    /// <summary>
    /// Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.
    /// </summary>
    public class LastChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to return</param>
        public LastChars(Func<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Invoke() ? value.Substring(value.Length - Length.Invoke(), Length.Invoke()) : value;
    }

    /// <summary>
    /// Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. 
    /// </summary>
    public class SkipFirstChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to skip</param>
        public SkipFirstChars(Func<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length <= Length.Invoke() ? new Empty().Keyword : value.Substring(Length.Invoke(), value.Length - Length.Invoke());
    }

    /// <summary>
    /// Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. 
    /// </summary>
    public class SkipLastChars : BaseTextLength
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to skip</param>
        public SkipLastChars(Func<int> length) 
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length <= Length.Invoke() ? new Empty().Keyword : value.Substring(0, value.Length - Length.Invoke());
    }

    public abstract class BasePaddingFunction : BaseTextLength
    {
        public Func<char> Character { get; }

        public BasePaddingFunction(Func<int> length, Func<char> character)
            : base(length)
            => Character = character;

        protected override object EvaluateEmpty() => new string(Character.Invoke(), Length.Invoke());
        protected override object EvaluateNull() => new string(Character.Invoke(), Length.Invoke());

    }

    /// <summary>
    /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. 
    /// </summary>
    public class PadRight : BasePaddingFunction
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the minimal length of the string returned</param>
        /// <param name="character">The padding character</param>
        public PadRight(Func<int> length, Func<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Invoke() ? value : value.PadRight(Length.Invoke(), Character.Invoke());
    }

    /// <summary>
    /// Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. 
    /// </summary>
    public class PadLeft : BasePaddingFunction
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the minimal length of the string returned</param>
        /// <param name="character">The padding character</param>
        public PadLeft(Func<int> length, Func<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Invoke() ? value : value.PadLeft(Length.Invoke(), Character.Invoke());
    }

    /// <summary>
    /// Returns the argument value except if this value only contains white-space characters then it returns `empty`.
    /// </summary>
    [Function(prefix:"", aliases: new[] {"blank-to-empty"})]
    public class WhitespacesToEmpty : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value only contains white-space characters then it returns `null`.
    /// </summary>
    [Function(prefix: "", aliases: new[] { "blank-to-null" })]
    public class WhitespacesToNull : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Null().Keyword;
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value is `empty` then it returns `null`.
    /// </summary>
    [Function(prefix: "")]
    public class EmptyToNull : BaseTextFunction
    {
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value is `null` then it returns `empty`.
    /// </summary>
    [Function(prefix: "")]
    public class NullToEmpty : BaseTextFunction
    {
        protected override object EvaluateNull() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value that has previously been HTML-encoded into a decoded string.
    /// </summary>
    [Function(prefix: "")]
    public class HtmlToText : BaseTextFunction
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlDecode(value);
    }

    /// <summary>
    /// Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. 
    /// </summary>
    public class Length : BaseTextFunction
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

    /// <summary>
    /// Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter.
    /// </summary>
    [Function(prefix: "")]
    public class TextToDateTime : BaseTextFunction
    {
        public Func<string> Format { get; }
        public Func<string> Culture { get; }

        /// <param name="format">A string representing the required format.</param>
        public TextToDateTime(Func<string> format)
            => (Format, Culture) = (format, () => string.Empty);

        /// <param name="format">A string representing the required format.</param>
        /// <param name="culture">A string representing a pre-defined culture.</param>
        public TextToDateTime(Func<string> format, Func<string> culture)
            => (Format, Culture) = (format, culture);

        protected override object EvaluateString(string value)
        {
            var info = (string.IsNullOrEmpty(Culture.Invoke()) ? CultureInfo.InvariantCulture : new CultureInfo(Culture.Invoke()!)).DateTimeFormat;

            if (DateTime.TryParseExact(value, Format.Invoke(), info, DateTimeStyles.RoundtripKind, out var dateTime))
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

            throw new ArgumentException($"Impossible to transform the value '{value}' into a date using the format '{Format}'");
        }
    }

    /// <summary>
    /// Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.
    /// </summary>
    public class RemoveChars : BaseTextFunction
    {
        public Func<char> CharToRemove { get; }

        /// <param name="charToRemove">The char to be removed from the argument string</param>
        public RemoveChars(Func<char> charToRemove)
            => CharToRemove = charToRemove;

        protected override object EvaluateString(string value)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in value)
                if (!c.Equals(CharToRemove.Invoke()))
                    stringBuilder.Append(c);
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
        {
            if (char.IsWhiteSpace(CharToRemove.Invoke()))
                return new Empty().Keyword;
            else
                return base.EvaluateBlank();
        }
    }

    /// <summary>
    /// Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced.
    /// </summary>
    [Function(prefix: "")]
    public class TextToMask : BaseTextFunction
    {
        private char maskChar { get; } = '*';
        public Func<string> Mask { get; }

        /// <param name="mask">The string representing the mask to apply to the argument string</param>
        public TextToMask(Func<string> mask)
            => Mask = mask;

        protected override object EvaluateString(string value)
        {
            var mask = Mask.Invoke() ?? string.Empty;
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
            => Mask.Invoke() ?? string.Empty;
        protected override object EvaluateEmpty()
            => Mask.Invoke() ?? string.Empty;
    }

    /// <summary>
    /// Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.
    /// </summary>
    [Function(prefix: "")]
    public class MaskToText : BaseTextFunction
    {
        private char maskChar { get; } = '*';
        public Func<string> Mask { get; }

        /// <param name="mask">The string representing the mask to be unset from the argument string</param>
        public MaskToText(Func<string> mask)
            => Mask = mask;

        protected override object EvaluateString(string value)
        {
            var mask = Mask.Invoke() ?? string.Empty;
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
            => ((Mask.Invoke() ?? string.Empty).Replace(maskChar.ToString(), "").Length == 0) ? new Whitespace().Keyword : new Null().Keyword;
        protected override object EvaluateEmpty()
            => ((Mask.Invoke() ?? string.Empty).Replace(maskChar.ToString(), "").Length == 0) ? new Empty().Keyword : new Null().Keyword;
    }
}
