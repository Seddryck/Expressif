using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    abstract class BaseNumericPredicate : BasePredicate
    {
        public override bool Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                DBNull => EvaluateNull(),
                decimal numeric => EvaluateNumeric(numeric),
                _ => EvaluateUncasted(value),
            };
        }
        protected bool EvaluateUncasted(object value)
        {
            if (new Null().Equals(value))
                return EvaluateNull();

            var caster = new NumericCaster();
            var numeric = caster.Execute(value);
            return EvaluateNumeric(numeric);
        }

        protected abstract bool EvaluateNumeric(decimal numeric);
    }

    abstract class BaseNumericPredicateReference : BaseNumericPredicate
    {
        public Func<decimal> Reference { get; }

        public BaseNumericPredicateReference(Func<decimal> reference)
            => Reference = reference;
    }
}