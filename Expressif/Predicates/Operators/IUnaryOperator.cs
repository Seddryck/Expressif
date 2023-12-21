using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators
{
    public interface IUnaryOperator : IPredicate, IOperator
    {
        IPredicate Member { get; }
    }
}
