using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace Expressif.Functions
{
    public class ChainPredicate : IPredicate
    {
        private IEnumerable<IPredicate> Predicates { get; }

        public ChainPredicate(IEnumerable<IPredicate> predicates)
            => Predicates = predicates;

        public bool Evaluate(object? value)
            => (bool)Predicates.Aggregate(value is IScalarResolver resolver ? resolver.Execute() : value, (v, func) => func.Evaluate(v) )!;
        
        object? IFunction.Evaluate(object? value) => throw new NotImplementedException();
    }
}
