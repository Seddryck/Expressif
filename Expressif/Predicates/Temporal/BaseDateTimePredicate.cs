using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    abstract class BaseDateTimePredicate : BasePredicate
    {
        public override bool Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                DBNull => EvaluateNull(),
                System.DateTime dt => EvaluateDateTime(dt),
                _ => EvaluateUncasted(value),
            };
        }
        protected bool EvaluateUncasted(object value)
        {
            if (new Null().Equals(value))
                return EvaluateNull();

            var caster = new DateTimeCaster();
            var dt = caster.Execute(value);
            return EvaluateDateTime(dt);
        }

        protected abstract bool EvaluateDateTime(System.DateTime dt);
    }

    abstract class BaseDateTimePredicateReference : BaseDateTimePredicate
    {
        public IScalarResolver<System.DateTime> Reference { get; }

        public BaseDateTimePredicateReference(IScalarResolver<System.DateTime> reference)
            => Reference = reference;
    }
}