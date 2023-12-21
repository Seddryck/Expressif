using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal;

[Predicate(prefix: "dateTime")]
public abstract class BaseDateTimePredicate : BasePredicate
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
    protected virtual bool EvaluateUncasted(object value)
    {
        if (new Null().Equals(value))
            return EvaluateNull();

        if (new DateTimeCaster().TryCast(value, out var dt))
            return EvaluateDateTime(dt);
        return false;
    }

    protected virtual bool EvaluateDate(DateOnly date)
        => EvaluateDateTime(date.ToDateTime(TimeOnly.MinValue));

    protected abstract bool EvaluateDateTime(DateTime dt);
}

public abstract class BaseDateTimePredicateReference : BaseDateTimePredicate
{
    public Func<DateTime> Reference { get; }

    public BaseDateTimePredicateReference(Func<DateTime> reference)
        => Reference = reference;
}