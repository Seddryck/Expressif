using Expressif.Functions;
using Expressif.Predicates.Combination;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace Expressif.Predicates
{
    public class CombinedPredicate : IPredicate
    {
        private IPredicate LeftMember { get; }
        private ICombinationOperator Operator { get; }
        private IPredicate RightMember { get; }

        public CombinedPredicate(IPredicate leftMember, ICombinationOperator @operator, IPredicate rightMember)
            => (LeftMember, Operator, RightMember) = (leftMember, @operator, rightMember);

        public bool Evaluate(object? value)
            => Operator.Evaluate(LeftMember, RightMember, value);
        
        object? IFunction.Evaluate(object? value) => throw new NotImplementedException();
    }
}
