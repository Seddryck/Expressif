using Expressif.Functions;
using Expressif.Predicates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif
{
    public class Predication : IPredicate
    {
        private readonly IPredicate predicate;

        public Predication(string code)
            : this(code, new Context()) { }
        public Predication(string code, Context context)
            : this(code, context, new PredicationFactory()) { }
        public Predication(string code, Context context, PredicationFactory factory)
            => predicate = factory.Instantiate(code, context);

        public virtual bool Evaluate(object? value) => predicate.Evaluate(value)!;

        object? IFunction.Evaluate(object? value) => predicate.Evaluate(value);
    }
}
