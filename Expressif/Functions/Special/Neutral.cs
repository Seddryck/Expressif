using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Special
{
    class Neutral : IFunction
    {
        public object Evaluate(object value) => value;
    }
}
