using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Library.Operators;

public interface IUnaryOperator : IPredicate, IOperator
{
    IPredicate Member { get; }
}
