using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators;

public interface IBinaryOperator: IPredicate, IOperator
{ }

public interface IBinaryOperator<P> : IBinaryOperator where P : IPredicate
{
    P LeftMember { get; }
    P RightMember { get; }
}
