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
    abstract class AbstractTextTransformation : IFunction
    {
        public object? Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                DBNull _ => EvaluateNull(),
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

    class TextToHtml : AbstractTextTransformation
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlEncode(value);
    }

    class TextToLower : AbstractTextTransformation
    {
        protected override object EvaluateString(string value) => value.ToLowerInvariant();
    }
    class TextToUpper : AbstractTextTransformation
    {
        protected override object EvaluateString(string value) => value.ToUpperInvariant();
    }

    class TextToTrim : AbstractTextTransformation
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value.Trim();
    }
    abstract class AbstractTextAppend : AbstractTextTransformation
    {
        public IScalarResolver<string> Append { get; }
        public AbstractTextAppend(IScalarResolver<string> append)
            => Append = append;

        protected override object EvaluateEmpty() => Append.Execute() ?? string.Empty;
        protected override object EvaluateBlank() => Append.Execute() ?? string.Empty;
    }

    class TextToPrefix : AbstractTextAppend
    {
        public TextToPrefix(IScalarResolver<string> prefix)
            : base(prefix) { }
        protected override object EvaluateString(string value) => $"{Append.Execute()}{value}";
    }

    class TextToSuffix : AbstractTextAppend
    {
        public TextToSuffix(IScalarResolver<string> suffix)
            : base(suffix) { }
        protected override object EvaluateString(string value) => $"{value}{Append.Execute()}";
    }

    abstract class AbstractTextLengthTransformation : AbstractTextTransformation
    {
        public IScalarResolver<int> Length { get; }

        public AbstractTextLengthTransformation(IScalarResolver<int> length)
            => Length = length;
    }

    class TextToFirstChars : AbstractTextLengthTransformation
    {
        public TextToFirstChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value.Substring(0, Length.Execute()) : value;
    }

    class TextToLastChars : AbstractTextLengthTransformation
    {
        public TextToLastChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value.Substring(value.Length - Length.Execute(), Length.Execute()) : value;
    }

    class TextToSkipFirstChars : AbstractTextLengthTransformation
    {
        public TextToSkipFirstChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length <= Length.Execute() ? new Empty().Keyword : value.Substring(Length.Execute(), value.Length - Length.Execute());
    }

    class TextToSkipLastChars : AbstractTextLengthTransformation
    {
        public TextToSkipLastChars(IScalarResolver<int> length)
            : base(length) { }

        protected override object EvaluateString(string value)
            => value.Length <= Length.Execute() ? new Empty().Keyword : value.Substring(0, value.Length - Length.Execute());
    }

    abstract class AbstractTextPadTransformation : AbstractTextLengthTransformation
    {
        public IScalarResolver<char> Character { get; }

        public AbstractTextPadTransformation(IScalarResolver<int> length, IScalarResolver<char> character)
            : base(length)
            => Character = character;

        protected override object EvaluateEmpty() => new string(Character.Execute(), Length.Execute());
        protected override object EvaluateNull() => new string(Character.Execute(), Length.Execute());

    }

    class TextToPadRight : AbstractTextPadTransformation
    {
        public TextToPadRight(IScalarResolver<int> length, IScalarResolver<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value : value.PadRight(Length.Execute(), Character.Execute());
    }

    class TextToPadLeft : AbstractTextPadTransformation
    {
        public TextToPadLeft(IScalarResolver<int> length, IScalarResolver<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Execute() ? value : value.PadLeft(Length.Execute(), Character.Execute());
    }

    [Function(false)]
    class BlankToEmpty : AbstractTextTransformation
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    [Function(false)]
    class BlankToNull : AbstractTextTransformation
    {
        protected override object EvaluateBlank() => new Null().Keyword;
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    [Function(false)]
    class EmptyToNull : AbstractTextTransformation
    {
        protected override object EvaluateEmpty() => new Null().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    [Function(false)]
    class NullToEmpty : AbstractTextTransformation
    {
        protected override object EvaluateNull() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    [Function(false)]
    class HtmlToText : AbstractTextTransformation
    {
        protected override object EvaluateString(string value) => WebUtility.HtmlDecode(value);
    }

    class TextToLength : AbstractTextTransformation
    {
        protected override object EvaluateSpecial(string value) => -1;
        protected override object EvaluateBlank() => -1;
        protected override object EvaluateEmpty() => 0;
        protected override object EvaluateNull() => 0;
        protected override object EvaluateString(string value) => value.Length;
    }

    class TextToToken : AbstractTextTransformation
    {
        public IScalarResolver<int> Index { get; }
        public IScalarResolver<char>? Separator { get; }
        public TextToToken(IScalarResolver<int> index)
            => (Index, Separator) = (index, null);
        public TextToToken(IScalarResolver<int> index, IScalarResolver<char> separator)
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

    class TextToTokenCount : TextToLength
    {
        public IScalarResolver<char>? Separator { get; }
        public TextToTokenCount()
            => Separator = null;
        public TextToTokenCount(IScalarResolver<char> separator)
            => Separator = separator;

        protected override object EvaluateBlank() => 0;
        protected override object EvaluateString(string value) => TokenCount(value);

        private int TokenCount(string value)
        {
            var tokenizer = Separator == null ? (ITokenizer)new WhitespaceTokenizer() : new Tokenizer(Separator.Execute());
            return tokenizer.Execute(value).Length;
        }
    }


    [Function(false)]
    class TextToDateTime : AbstractTextTransformation
    {
        public IScalarResolver<string> Format { get; }
        public IScalarResolver<string> Culture { get; }

        public TextToDateTime(IScalarResolver<string> format)
            => (Format, Culture) = (format, new LiteralScalarResolver<string>(string.Empty));

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

    class TextToRemoveChars : AbstractTextTransformation
    {
        public IScalarResolver<char> CharToRemove { get; }
        public TextToRemoveChars(IScalarResolver<char> charToRemove)
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

    class TextToMask : AbstractTextTransformation
    {
        private char maskChar { get; } = '*';
        public IScalarResolver<string> Mask { get; }
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

    [Function(false)]
    class MaskToText : AbstractTextTransformation
    {
        private char maskChar { get; } = '*';
        public IScalarResolver<string> Mask { get; }
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
