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
    [Predicate(prefix: "dateTime")]
    abstract class BaseDateTimePredicate : BasePredicate
    {
        public override bool Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                DBNull => EvaluateNull(),
                DateTime dt => EvaluateDateTime(dt),
                DateOnly d => EvaluateDate(d),
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

        protected virtual bool EvaluateDate(DateOnly date) => EvaluateDateTime(date.ToDateTime(TimeOnly.MinValue));

        protected abstract bool EvaluateDateTime(DateTime dt);
    }

    abstract class BaseDateTimePredicateReference : BaseDateTimePredicate
    {
        public Func<DateTime> Reference { get; }

        public BaseDateTimePredicateReference(Func<DateTime> reference)
            => Reference = reference;
    }
}