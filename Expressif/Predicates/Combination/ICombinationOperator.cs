using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    public interface ICombinationOperator
    {
        bool Evaluate(IPredicate left, IPredicate right, object? value);
    }
}
