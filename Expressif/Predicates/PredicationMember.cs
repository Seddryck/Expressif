using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates;
using Expressif.Predicates.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    public record class PredicationMember(Type? Operator, Type Predicate, object[] Parameters)
    {
        public PredicationMember(Type predicate, object[] parameters)
            : this(null, predicate, parameters) { }

        public (ICombinationOperator?, IPredicate) Build(Context context, PredicationFactory factory)
        {
            var typedParameters = new List<IParameter>();
            foreach (var parameter in Parameters)
            {
                typedParameters.Add(parameter switch
                {
                    IParameter p => p,
                    _ => new LiteralParameter(parameter.ToString()!)
                });
            }
            var @operator = Operator==null ? null : factory.Instantiate(Operator);
            var predicate = factory.Instantiate(Predicate, typedParameters.ToArray(), context);
            return (@operator, predicate);
        }
    };
}
