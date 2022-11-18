using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers
{
    public class PredicateExpression
    {
        public Function Member { get; }

        public PredicateExpression(Function member)
            => (Member) = (member);

        public static readonly Parser<PredicateExpression> Parser =
            from predicate in Function.Parser
            select new PredicateExpression(predicate);
    }
}
