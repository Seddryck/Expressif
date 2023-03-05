using Expressif.Values;
using Expressif.Values.Special;
using Sprache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Parsers
{
    public class Grammar
    {
        public static readonly char[] OpeningQuotedChars = new[] { '\"', '@', ',', '(', '[', '{'};
        public static readonly char[] ClosingQuotedChars = new[] { '\"', '@', ',', ')', ']', '}'};
        public static readonly char[] AlongQuotedChars = new[] { '\"', '@', ',', '|', ' '};

        public static readonly Parser<string> FunctionName =
            from tokens in Parse.Letter.AtLeastOnce().Text().DelimitedBy(Parse.Char('-')).Token()
            select string.Join('-', tokens.ToArray());

        public static readonly Parser<char> Delimitator = Parse.Char('|').Token();

        public static readonly Parser<string> Variable =
            from arobas in Parse.Char('@').Token()
            from id in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token()
            select id;

        private static readonly Parser<string> UnquotedLiteral =
            from firstChar in Parse.CharExcept(OpeningQuotedChars.Union(AlongQuotedChars)).Token()
            from otherChars in Parse.CharExcept(ClosingQuotedChars.Union(AlongQuotedChars)).Many().Token().Optional()
            select string.Concat(firstChar.ToString().Concat(otherChars.GetOrElse(string.Empty)));

        private static readonly Parser<string> QuotedLiteral =
            Parse.CharExcept("\"").AtLeastOnce().Text().Contained(Parse.Char('\"'), Parse.Char('\"')).Token();

        public static readonly Parser<string> Literal = UnquotedLiteral.Or(QuotedLiteral);
    }
}
