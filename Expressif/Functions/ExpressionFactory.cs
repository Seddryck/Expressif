using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Values;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Expressif.Functions
{
    public class ExpressionFactory
    {
        private Parser<Expression> Parser { get; } = Expression.Parser;
        private FunctionFactory FunctionFactory { get; } = new FunctionFactory();
        

        public IEnumerable<IFunction> Instantiate(string code, Context context)
        {
            var expression = Parser.Parse(code);

            foreach (var member in expression.Members)
                yield return FunctionFactory.Instantiate(member.Name, member.Parameters, context);
        }
    }
}
