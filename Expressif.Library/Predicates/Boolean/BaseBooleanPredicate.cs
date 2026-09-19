using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            int integer => EvaluateInteger(integer),
            decimal integer => EvaluateDecimal(integer),
            _ => EvaluateUncasted(value),
        };
    }
    protected bool EvaluateUncasted(object value)
    {
        if (new Null().Equals(value))
            return EvaluateNull();

        var caster = new BooleanCaster();
        if (caster.TryCast(value, out var boolean))
        {
            return EvaluateBoolean(boolean);
        }
        else
        {
            var numericCaster = new NumericCaster();
            if (numericCaster.TryCast(value, out var numeric))
                return EvaluateDecimal(numeric);
            return EvaluateNull();
        }
    }

    protected abstract bool EvaluateBoolean(bool boolean);
    protected bool EvaluateInteger(int integer)
        => EvaluateBoolean(integer != 0);
    protected bool EvaluateDecimal(decimal numeric)
        => EvaluateBoolean(numeric != 0);
}
