using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Values;
using Expressif.Values.Resolvers;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expressif.Functions
{
    public class ExpressionFactory : BaseExpressionFactory
    {
        private Parser<Parsers.Expression> Parser { get; } = Parsers.Expression.Parser;

        public IFunction Instantiate(string code, Context context)
        {
            var expression = Parser.Parse(code);

            var functions = new List<IFunction>();
            foreach (var member in expression.Members)
                functions.Add(Instantiate<IFunction>(member.Name, member.Parameters, context));
            return new ChainFunction(functions);
        }

        public IFunction Instantiate(Type type, IParameter[] parameters, Context context)
            => Instantiate<IFunction>(type, parameters, context);

        internal Type GetFunctionType(string functionName)
            => GetFunctionType<IFunction>(functionName);
    }
}
