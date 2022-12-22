using Expressif.Parsers;
using Expressif.Predicates.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    internal record class SubPredicationMember(Type? Operator, PredicationBuilder PredicationBuilder)
    {
        public (ICombinationOperator?, IPredicate) Build(Context context, PredicationFactory factory)
        {
            var @operator = Operator == null ? null : factory.Instantiate<ICombinationOperator>(Operator);
            var predicate = PredicationBuilder.Build()!;
            return (@operator, predicate);
        }
    }
}
