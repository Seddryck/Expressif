using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Library.Operators;

public interface IBinaryOperator : IPredicate, IOperator
{
    IPredicate LeftMember { get; }
    IPredicate RightMember { get; }
}
