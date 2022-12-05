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
        private readonly IPredicate expression;

        public Predication(string code)
            : this(code, new Context()) { }
        public Predication(string code, Context context)
            : this(code, context, new PredicationFactory()) { }
        public Predication(string code, Context context, PredicationFactory factory)
            => expression = factory.Instantiate(code, context);

        public virtual bool Evaluate(object? value) => expression.Evaluate(value)!;
        object? IFunction.Evaluate(object? value) => expression.Evaluate(value);
    }
}
