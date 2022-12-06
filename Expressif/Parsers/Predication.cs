using Expressif.Functions;
using Expressif.Functions.Special;
using Expressif.Predicates.Boolean;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers
{
    public interface IPredication { }

    public class BasicPredication : IPredication
    {
        public Function[] Members { get; }
        
        public BasicPredication(Function[] members)
            => (Members) = (members);

        private static readonly Parser<BasicPredication> PositiveParser =
            from predicate in Function.Parser
            select new BasicPredication(new[] { predicate });

        private static readonly Parser<BasicPredication> NegativeParser =
            from negation in Parse.Char('!').Token()
            from predicate in Function.Parser
            select new BasicPredication(new[] { new Function(nameof(False), Array.Empty<IParameter>()), predicate });

        public static readonly Parser<BasicPredication> Parser = NegativeParser.Or(PositiveParser);
    }

    public class Operator
    {
        public string Name { get; }

        public Operator(string name)
            => (Name) = (name);

        public static readonly Parser<Operator> Parser =
            from _ in Parse.Char('|')
            from @operator in Keyword.OrOperator.Or(Keyword.AndOperator).Or(Keyword.XorOperator)
            select new Operator(@operator);
    }

    class SubPredication : IPredication
    {
        public static readonly Parser<IPredication> Parser =
            from open in Parse.Char('{').Token()
            from chain in Predication.Parser
            from close in Parse.Char('}').Token()
            select chain;
    }

    public class Predication : IPredication
    {
        public IPredication LeftMember { get; }
        public Operator Operator { get; }
        public IPredication RightMember { get; }

        public Predication(IPredication leftMember, Operator @operator, IPredication rightMember)
            => (LeftMember, Operator, RightMember) = (leftMember, @operator, rightMember);

        public static readonly Parser<IPredication> Parser =
            Parse.ChainOperator(Operator.Parser, SubPredication.Parser.Or(BasicPredication.Parser), (op, left, right) => new Predication(left, op, right));
    }


    public class InputPredication
    {
        public IPredication Predication { get; }
        public IParameter Parameter { get; }

        public InputPredication(IParameter parameter, IPredication member)
            => (Parameter, Predication) = (parameter, member);

        public static readonly Parser<InputPredication> Parser =
            from parameter in Parsers.Parameter.Parser.Token()
            from _ in Parse.IgnoreCase("|?").Token()
            from predication in Parsers.Predication.Parser.Token()
            select new InputPredication(parameter, predication);
    }
}
    