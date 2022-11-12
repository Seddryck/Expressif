using Expressif.Values.Special;
using Sprache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Parsers
{
    public class Grammar
    {
        private static readonly Parser<string> FunctionNameToken =
            from letters in Parse.Letter.AtLeastOnce().Text()
            from dash in Parse.Char('-').Once().Text()
            select string.Concat(letters.Concat(dash));

        public static readonly Parser<string> FunctionName =
            from tokens in FunctionNameToken.Many().Optional()
            from lastToken in Parse.Letter.AtLeastOnce().Text().Token()
            select string.Concat(tokens.GetOrElse(Enumerable.Empty<string>()).Append(lastToken));

        public static readonly Parser<char> Delimitator = Parse.Char('|').Token();

        public static readonly Parser<string> Variable =
            from arobas in Parse.Char('@').Token()
            from firstLetter in Parse.Letter.Once().Text()
            from followingLetters in Parse.LetterOrDigit.Many().Text().Token().Optional()
            select string.Concat(firstLetter.Concat(followingLetters.GetOrElse(string.Empty)));

        private static readonly Parser<string> UnquotedLiteral =
            from firstChar in Parse.CharExcept(new[] { '\"', '@', ',', '(', '[', '{', ' ' }).Token()
            from otherChars in Parse.CharExcept(new[] { ',', '|', ')', ']', '}', ' ' }).Many().Token().Optional()
            select string.Concat(firstChar.ToString().Concat(otherChars.GetOrElse(string.Empty)));
        
        private static readonly Parser<string> QuotedLiteral =
            Parse.CharExcept("\"").AtLeastOnce().Text().Contained(Parse.Char('\"'), Parse.Char('\"')).Token();

        public static readonly Parser<string> Literal = UnquotedLiteral.Or(QuotedLiteral);
    }
}
