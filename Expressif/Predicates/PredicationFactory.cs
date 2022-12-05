using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates.Combination;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    public class PredicationFactory : BaseExpressionFactory
    {
        private Parser<IPredication> Parser { get; } = Parsers.Predication.Parser;

        public virtual IPredicate Instantiate(string code, Context context)
        {
            var rootPredication = Parser.Parse(code);
            var predicate = Instantiate(rootPredication, context);
            return predicate;
        }

        public IPredicate Instantiate(IPredication predication, Context context)
        => predication switch
        {
            Parsers.BasicPredication basic => Instantiate(basic, context),
            Parsers.Predication combined => new CombinedPredicate(Instantiate(combined.LeftMember, context), Instantiate(combined.Operator.Name), Instantiate(combined.RightMember, context)),
            _ => throw new NotImplementedException()
        };

        public IPredicate Instantiate(BasicPredication basic, Context context)
        {
            var predicates = new List<IPredicate>();
            foreach (var predicate in basic.Members)
                predicates.Add(Instantiate<IPredicate>(predicate.Name, predicate.Parameters, context));

            return new ChainPredicate(predicates);
        }

        public ICombinationOperator Instantiate(string operatorName)
            => Instantiate<ICombinationOperator>(GetFunctionType<ICombinationOperator>($"{operatorName}-operator"), Array.Empty<IParameter>(), new Context());

        public IPredicate Instantiate(Type type, IParameter[] parameters, Context context)
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
