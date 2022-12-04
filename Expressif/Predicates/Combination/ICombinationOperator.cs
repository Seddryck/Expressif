using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    interface ICombinationOperator
    {
        bool Evaluate(bool state, object? value);
    }
}
