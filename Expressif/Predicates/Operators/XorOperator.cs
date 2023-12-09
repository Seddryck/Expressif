using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators
{
    [Operator]
    internal class XorOperator : IBinaryOperator
    {
        public IPredicate LeftMember { get; }
        public IPredicate RightMember { get; }

        public XorOperator(IPredicate leftMember, IPredicate rightMember)
            => (LeftMember, RightMember) = (leftMember, rightMember);

        public bool Evaluate(object? value)
            => LeftMember.Evaluate(value) ^ RightMember.Evaluate(value);
        object? IFunction.Evaluate(object? value) => Evaluate(value);
    }
}
