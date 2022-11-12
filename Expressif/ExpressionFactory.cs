using Expressif.Functions;
using Expressif.Parsers;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Expressif
{
    public class ExpressionFactory
    {
        private Parser<Expression> Parser { get; } = Expression.Parser;
        private FunctionFactory Factory { get; } = new FunctionFactory();

        public IEnumerable<IFunction> Instantiate(string code)
        {
            var expression = Parser.Parse(code);

            foreach (var member in expression.Members)
                yield return Factory.Instantiate(member.Name, member.Parameters);
        }
    }
}
