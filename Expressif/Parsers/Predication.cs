using Expressif.Predicates.Boolean;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers
{
    public class Predication
    {
        public Function[] Members { get; }

        public Predication(Function[] members)
            => (Members) = (members);

        
        private static readonly Parser<Predication> PositiveParser =
            from predicate in Function.Parser
            select new Predication(new[] { predicate });

        private static readonly Parser<Predication> NegativeParser =
            from negation in Parse.Char('!').Token()
            from predicate in Function.Parser
            select new Predication(new[] { new Function(nameof(False), Array.Empty<IParameter>()), predicate });
        
        public static readonly Parser<Predication> Parser = NegativeParser.Or(PositiveParser);

    }
}
