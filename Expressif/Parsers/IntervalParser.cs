using Expressif.Values;
using Sprache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers;

public class IntervalParser
{
    protected static readonly Parser<IntervalMeta> Classic =
        from lowerBoundType in Parse.Chars(']', '[').Token()
        from lowerBound in Parse.IgnoreCase("-INF").Or(Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce()).Text()
        from separator in Parse.Char(';')
        from upperBound in Parse.IgnoreCase("+INF").Or(Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce()).Text()
        from upperBoundType in Parse.Chars(']', '[').Token()
        select new IntervalMeta(lowerBoundType, lowerBound, upperBound, upperBoundType);

    protected static readonly Parser<IntervalMeta> ZeroBasedShorthand =
        from lp in Parse.Chars('(').Token()
        from zero in Parse.Chars('0').Token().Optional()
        from sign in Parse.Char('+').Or(Parse.Char('-'))
        from rp in Parse.Chars(')').Token()
        select new IntervalMeta(
            sign == '+' && !zero.IsDefined ? ']' : '[',
            sign == '+' ? "0" : "-INF",
            sign == '+' ? "+INF" : "0",
            sign == '-' && !zero.IsDefined ? '[' : ']'
        );

    protected static readonly Parser<IntervalMeta> ZeroBasedLonghand =
        from lp in Parse.Chars('(').Token()
        from absolutely in Parse.IgnoreCase("absolutely-").Optional()
        from sign in Parse.IgnoreCase("positive").Return('+').Or(Parse.IgnoreCase("negative").Return('-'))
        from rp in Parse.Chars(')').Token()
        select new IntervalMeta(
            sign == '+' && absolutely.IsDefined ? ']' : '[',
            sign == '+' ? "0" : "-INF",
            sign == '+' ? "+INF" : "0",
            sign == '-' && absolutely.IsDefined ? '[' : ']'
        );

    protected static readonly Parser<IntervalMeta> NonZeroBasedShorthand =
        from lp in Parse.Chars('(').Token()
        from sign in Parse.Char('>').Or(Parse.Char('<'))
        from equal in Parse.Chars('=').Token().Optional()
        from bound in Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce().Text()
        from rp in Parse.Chars(')').Token()
        select new IntervalMeta(
            sign == '>' && !equal.IsDefined ? ']' : '[',
            sign == '>' ? bound : "-INF",
            sign == '>' ? "+INF" : bound,
            sign == '<' && !equal.IsDefined ? '[' : ']'
        );

    public static readonly Parser<IntervalMeta> Parser =
        Classic.Or(ZeroBasedShorthand).Or(NonZeroBasedShorthand).Or(ZeroBasedLonghand);
}
