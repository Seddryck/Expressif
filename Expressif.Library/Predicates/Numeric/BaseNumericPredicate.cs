using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Predicates.Numeric;

public abstract class BaseNumericPredicate : BasePredicate
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
        if (caster.TryCast(value, out var numeric))
            return EvaluateNumeric(numeric);
        return EvaluateNull();
    }

    protected abstract bool EvaluateNumeric(decimal numeric);
}

public abstract class BaseNumericPredicateReference : BaseNumericPredicate
{
    public Func<decimal> Reference { get; }

    public BaseNumericPredicateReference(Func<decimal> reference)
        => Reference = reference;
}
