using Expressif.Functions;
using Sprache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Expressif.Parsers
{
    public interface IExpression { }

    public class Expression : IExpression
    {
        public IEnumerable<Function> Members { get; }

        public Expression(IEnumerable<Function> members)
            => (Members) = (members);

        public static readonly Parser<Expression> Parser =
            from first in Function.Parser.Once()
            from others in (
                from _ in Parse.Char('|').Token()
                from p in Function.Parser.Token()
                select p).Many()
            select new Expression(first.Concat(others));
    }

    public class InputExpression
    {
        public IEnumerable<Function> Members { get; }
        public IParameter Parameter { get; }

        public InputExpression(IParameter parameter, IEnumerable<Function> members)
            => (Parameter, Members) = (parameter, members);

        public static readonly Parser<InputExpression> Parser =
            from parameter in Parsers.Parameter.Parser.Token()
            from remaining in (
                from _ in Parse.Char('|').Token()
                from expression in Expression.Parser
                select expression.Members
            ).Optional()
            select new InputExpression(parameter, remaining.GetOrElse(Enumerable.Empty<Function>()));
    }
}
