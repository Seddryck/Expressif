using Expressif.Functions;
using Expressif.Parsers;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    public class PredicationFactory : BaseExpressionFactory
    {
        private Parser<Parsers.Predication> Parser { get; } = Parsers.Predication.Parser;

        public IPredicate Instantiate(string code, Context context)
        {
            var expression = Parser.Parse(code);
            var predicates = new List<IPredicate>();
            foreach (var member in expression.Members)
                predicates.Add(Instantiate<IPredicate>(member.Name, member.Parameters, context));
            return new ChainPredicate(predicates);
        }

        public IFunction Instantiate(Type type, IParameter[] parameters, Context context)
            => Instantiate<IPredicate>(type, parameters, context);

        internal Type GetFunctionType(string functionName)
            => GetFunctionType<IPredicate>(functionName);

        protected override Type GetFunctionType<T>(string functionName)
        {
            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            var className = textInfo.ToTitleCase(functionName.Trim());

            var tokens = functionName.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (tokens.Length>1 && tokens[1].Equals("is", StringComparison.InvariantCultureIgnoreCase))
            {
                var @namespace = tokens[0].Replace("Datetime", "DateTime").Replace("Timespan", "TimeSpan");
                var shortClassName = string.Concat(tokens.Skip(2));
                return typeof(T).Assembly.GetTypes()
                       .Where(
                                t => t.IsClass
                                && t.IsAbstract == false
                                && t.Name.Equals(shortClassName, StringComparison.InvariantCultureIgnoreCase)
                                && t.Namespace!.EndsWith(@namespace, StringComparison.InvariantCultureIgnoreCase)
                                && t.GetInterface(typeof(T).Name) != null)
                       .SingleOrDefault()
                       ?? throw new NotImplementedFunctionException(className);
            }
            else
                return base.GetFunctionType<T>(functionName);
        }

    }
}
