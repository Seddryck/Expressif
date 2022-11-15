using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions
{
    public class ExpressionFunction : IFunction
    {
        private IEnumerable<IFunction> Functions { get; }

        public ExpressionFunction(IEnumerable<IFunction> functions)
            => Functions = functions;

        public object? Evaluate(object? value)
            => Functions.Aggregate(value is IScalarResolver resolver ? resolver.Execute() : value, (v, func) => func.Evaluate(v) );
    }
}
