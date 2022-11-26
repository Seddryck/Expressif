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

    public class InputPredication
    {
        public Predication Member { get; }
        public IParameter Parameter { get; }

        public InputPredication(IParameter parameter, Predication member)
            => (Parameter, Member) = (parameter, member);

        public static readonly Parser<InputPredication> Parser =
            from parameter in Parsers.Parameter.Parser.Token()
            from _ in Parse.IgnoreCase("|?").Token()
            from predication in Predication.Parser.Token().End()
            select new InputPredication(parameter, predication);
    }
}
