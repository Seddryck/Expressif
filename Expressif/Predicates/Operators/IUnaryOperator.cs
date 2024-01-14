using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators;

public interface IUnaryOperator : IPredicate, IOperator
{ }

public interface IUnaryOperator<P> : IUnaryOperator where P : IPredicate
{
    P Member { get; }
}
