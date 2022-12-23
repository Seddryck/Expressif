using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates;
using Expressif.Predicates.Combination;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    public record class PredicationMember(Type? Operator, Type Negation, Type Predicate, object?[] Parameters)
    {
        public PredicationMember(Type predicate, object?[] parameters)
            : this(null, typeof(EverOperator), predicate, parameters) { }

        public PredicationMember(Type @operator, Type predicate, object?[] parameters)
            : this(@operator, typeof(EverOperator), predicate, parameters) { }

        public (ICombinationOperator?, IPredicate) Build(Context context, PredicationFactory factory)
        {
            var typedParameters = new List<IParameter>();
            foreach (var parameter in Parameters)
            {
                typedParameters.Add(parameter switch
                {
                    IParameter p => p,
                    _ => new LiteralParameter(parameter?.ToString() ?? new Null().Keyword)
                });
            }
            var @operator = Operator==null ? null : factory.Instantiate<ICombinationOperator>(Operator);
            var predicate = factory.Instantiate(Negation, Predicate, typedParameters.ToArray(), context);
            return (@operator, predicate);
        }
    };
}
