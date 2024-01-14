using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators;

[Operator]
internal class OrOperator<P> : IBinaryOperator<P> where P : IPredicate
{
    public P LeftMember { get; }
    public P RightMember { get; }

    public OrOperator(P leftMember, P rightMember)
        => (LeftMember, RightMember) = (leftMember, rightMember);

    public bool Evaluate(object? value)
        => LeftMember!.Evaluate(value) || RightMember!.Evaluate(value);
    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
