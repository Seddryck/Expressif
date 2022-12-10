using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates.Combination;
using Expressif.Predicates.Introspection;
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

        protected override IDictionary<string, Type> Initialize()
        {
            var introspector = new PredicateIntrospector();
            var infos = introspector.Locate();
            var mapping = new Dictionary<string, Type>();
            foreach (var info in infos)
            {
                mapping.Add(info.Name, info.ImplementationType);
                foreach (var alias in info.Aliases)
                    mapping.Add(alias, info.ImplementationType);
            }
            return mapping;
        }
    }
}
