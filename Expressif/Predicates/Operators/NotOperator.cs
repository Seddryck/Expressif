using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Predicates.Operators;

[Operator(["!"])]
internal class NotOperator<P> : IUnaryOperator<P> where P : IPredicate
{
    public P Member { get; }

    public NotOperator(P member)
        => (Member) = (member);

    public bool Evaluate(object? value)
        => !Member.Evaluate(value);

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
