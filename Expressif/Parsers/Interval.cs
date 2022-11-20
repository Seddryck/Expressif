using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers
{
    public class Interval
    {
        public static readonly Parser<Interval> Parser =
            from lowerBoundType in Parse.Chars(']', '[').Token()
            from lowerBound in Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce().Text()
            from separator in Parse.Char(';')
            from upperBound in Parse.Numeric.Or(Parse.Chars('.', '-', ':', ' ')).AtLeastOnce().Text()
            from upperBoundType in Parse.Chars(']', '[').Token()
            select new Interval(lowerBoundType, lowerBound, upperBound, upperBoundType);

        public string LowerBound { get; }
        public string UpperBound { get; }
        public char LowerBoundType { get; }
        public char UpperBoundType { get; }

        public Interval(char lowerBoundType, string lowerBound, string upperBound, char upperBoundType)
        {
            (LowerBoundType, LowerBound, UpperBound, UpperBoundType) = (lowerBoundType, lowerBound, upperBound, upperBoundType);
        }
    }
}
