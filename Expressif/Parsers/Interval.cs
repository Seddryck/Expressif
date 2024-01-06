using Sprache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers;

public class Interval
{
    
    public string LowerBound { get; }
    public string UpperBound { get; }
    public char LowerBoundType { get; }
    public char UpperBoundType { get; }

    public Interval(char lowerBoundType, string lowerBound, string upperBound, char upperBoundType)
    {
        (LowerBoundType, LowerBound, UpperBound, UpperBoundType) = (lowerBoundType, lowerBound, upperBound, upperBoundType);
    }

    protected static readonly Parser<Interval> Classic =
        from lowerBoundType in Parse.Chars(']', '[').Token()
        from lowerBound in Parse.IgnoreCase("-INF").Or(Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce()).Text()
        from separator in Parse.Char(';')
        from upperBound in Parse.IgnoreCase("+INF").Or(Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce()).Text()
        from upperBoundType in Parse.Chars(']', '[').Token()
        select new Interval(lowerBoundType, lowerBound, upperBound, upperBoundType);

    protected static readonly Parser<Interval> ZeroBasedShorthand =
        from lp in Parse.Chars('(').Token()
        from zero in Parse.Chars('0').Token().Optional()
        from sign in Parse.Char('+').Or(Parse.Char('-'))
        from rp in Parse.Chars(')').Token()
        select new Interval(
            sign == '+' && !zero.IsDefined ? ']' : '[',
            sign == '+' ? "0" : "-INF",
            sign == '+' ? "+INF" : "0",
            sign == '-' && !zero.IsDefined ? '[' : ']'
        );

    protected static readonly Parser<Interval> ZeroBasedLonghand =
        from lp in Parse.Chars('(').Token()
        from absolutely in Parse.IgnoreCase("absolutely-").Optional()
        from sign in Parse.IgnoreCase("positive").Return('+').Or(Parse.IgnoreCase("negative").Return('-'))
        from rp in Parse.Chars(')').Token()
        select new Interval(
            sign == '+' && absolutely.IsDefined ? ']' : '[',
            sign == '+' ? "0" : "-INF",
            sign == '+' ? "+INF" : "0",
            sign == '-' && absolutely.IsDefined ? '[' : ']'
        );

    protected static readonly Parser<Interval> NonZeroBasedShorthand =
        from lp in Parse.Chars('(').Token()
        from sign in Parse.Char('>').Or(Parse.Char('<'))
        from equal in Parse.Chars('=').Token().Optional()
        from bound in Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce().Text()
        from rp in Parse.Chars(')').Token()
        select new Interval(
            sign == '>' && !equal.IsDefined ? ']' : '[',
            sign == '>' ? bound : "-INF",
            sign == '>' ? "+INF" : bound,
            sign == '<' && !equal.IsDefined ? '[' : ']'
        );

    public static readonly Parser<Interval> Parser =
        Classic.Or(ZeroBasedShorthand).Or(NonZeroBasedShorthand).Or(ZeroBasedLonghand);
}
