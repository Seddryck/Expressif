using Expressif.Values.Casters;
using Expressif.Values.Special;
using System.Collections;

namespace Expressif.Predicates.Array;

public abstract class BaseArrayPredicate : BasePredicate
{
    public override bool Evaluate(object? value)
    {
        if (value is null or DBNull || new Null().Equals(value) || new Empty().Equals(value))
            return EvaluateNull();

        if (value is IEnumerable enumerable and not string)
            return EvaluateArray(enumerable);

        if (value is string text && new ArrayCaster().TryParse(text, out var array))
            return EvaluateArray(array);

        return EvaluateNull();
    }

    protected abstract bool EvaluateArray(IEnumerable array);
}
