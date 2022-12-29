using Expressif.Functions;
using Expressif.Functions.Introspection;
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

        protected override IDictionary<string, Type> Initialize()
        {
            var introspector = new FunctionIntrospector();
            var infos = introspector.Locate();
            var mapping = new Dictionary<string, Type>();
            foreach (var info in infos)
            {
                mapping.Add(info.Name, info.ImplementationType);
                foreach (var alias in info.Aliases)
                    if (mapping.TryGetValue(alias, out var existing))
                        throw new InvalidOperationException($"The function name '{alias}' has already been added for the implementation '{existing.FullName}'. You cannot add a second times this alias for the implementation '{info.ImplementationType.FullName}'");
                    else
                        mapping.Add(alias, info.ImplementationType);
            }
            return mapping;
        }
    }
}
