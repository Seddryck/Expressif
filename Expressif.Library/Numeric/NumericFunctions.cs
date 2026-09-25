using System;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Library.Numeric;

[Function]
public abstract class BaseNumericFunction : IFunction<decimal?, decimal?>
{
    public BaseNumericFunction()
    { }

    public decimal? Evaluate(decimal? value)
        => value.HasValue ? EvaluateNumeric(value.Value) : EvaluateTypedNull();

    public object? Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull _ => EvaluateNull(),
            decimal numeric => EvaluateNumeric(numeric),
            _ => EvaluateUncasted(value),
        };
    }

    protected virtual object? EvaluateUncasted(object value)
    {
        if (Expressif.Values.Special.Null.Instance.Equals(value) || Expressif.Values.Special.Empty.Instance.Equals(value) || Whitespace.Instance.Equals(value))
            return EvaluateNull();

        return new NumericCaster().TryCast(value, out var numeric)
            ? EvaluateNumeric(numeric)
            : EvaluateNull();
    }

    protected virtual object? EvaluateNull() => null;
    private decimal? EvaluateTypedNull()
        => EvaluateNull() is object value && new NumericCaster().TryCast(value, out var numeric)
            ? numeric
            : null;

    protected abstract decimal? EvaluateNumeric(decimal numeric);
}
