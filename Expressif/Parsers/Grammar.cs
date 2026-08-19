using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expressif.Values;
using Expressif.Values.Special;
using Sprache;

namespace Expressif.Parsers;

public class Grammar
{
    public static readonly char[] OpeningQuotedChars = ['\"', '`', '@', ',', '(', '[', '{'];
    public static readonly char[] ClosingQuotedChars = ['\"', '`', '@', ',', ')', ']', '}'];
    public static readonly char[] AlongQuotedChars = ['\"', '`', '@', ',', '|', ' '];

    public static readonly Parser<string> FunctionName =
        from tokens in Parse.Letter.AtLeastOnce().Text().DelimitedBy(Parse.Char('-')).Token()
        select string.Join('-', tokens.ToArray());

    public static readonly Parser<char> Delimitator = Parse.Char('|').Token();

    public static readonly Parser<string> Variable =
        from arobas in Parse.Char('@').Token()
        from id in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit).Token()
        select id;

    protected static readonly Parser<string> UnquotedLiteral =
        from firstChar in Parse.CharExcept(OpeningQuotedChars.Union(AlongQuotedChars)).Token()
        from otherChars in Parse.CharExcept(ClosingQuotedChars.Union(AlongQuotedChars)).Many().Token().Optional()
        select string.Concat(firstChar.ToString().Concat(otherChars.GetOrElse(string.Empty)));

    protected static readonly Parser<string> DoubleQuotedLiteral =
        Parse.Regex("\"(?:\\\\.|[^\"\\\\])*\"").Token()
            .Select(x => RecordSyntax.UnescapeDoubleQuoted(x[1..^1]));

    protected static readonly Parser<string> BacktickQuotedLiteral =
        Parse.CharExcept("`").Many().Text().Contained(Parse.Char('`'), Parse.Char('`')).Token();

    protected static readonly Parser<string> QuotedLiteral =
        DoubleQuotedLiteral.Or(BacktickQuotedLiteral);

    public static readonly Parser<string> Quoted = QuotedLiteral;

    public static readonly Parser<string> BareToken =
        Parse.Regex("[A-Za-z0-9_+\\-]+").Token();

    public static readonly Parser<string> Literal = UnquotedLiteral.Or(QuotedLiteral);
}
