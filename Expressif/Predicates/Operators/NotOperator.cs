using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Predicates.Operators
{
    [Operator(["!"])]
    internal class NotOperator : IUnaryOperator
    {
        public IPredicate Member { get; }

        public NotOperator(IPredicate member)
            => (Member) = (member);

        public bool Evaluate(object? value)
            => !Member.Evaluate(value);

        object? IFunction.Evaluate(object? value) => Evaluate(value);
    }
}
