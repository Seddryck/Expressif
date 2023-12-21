using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Boolean;

public abstract class BaseBooleanPredicate : BasePredicate
{
    public override bool Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull => EvaluateNull(),
            bool boolean => EvaluateBoolean(boolean),
            _ => EvaluateUncasted(value),
        };
    }
    protected bool EvaluateUncasted(object value)
    {
        if (new Null().Equals(value))
            return EvaluateNull();

        var caster = new BooleanCaster();
        var boolean = caster.Cast(value);
        return EvaluateBoolean(boolean);
    }

    protected abstract bool EvaluateBoolean(bool boolean);
}