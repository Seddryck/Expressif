using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Special
{
    /// <summary>
    /// Returns the argument value.
    /// </summary>
    [Function(prefix: "Special")]
    public class Neutral : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
