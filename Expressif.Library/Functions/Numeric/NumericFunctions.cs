using System;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Numeric;

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
        if (new Null().Equals(value) || new Empty().Equals(value) || new Whitespace().Equals(value))
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

/// <summary>
/// Returns the unmodified argument value except if the argument value is `null`, `empty` or `whitespace` then it returns `0`.
/// </summary>
[Function(prefix: "")]
[Scope("numeric/conversion")]
public class NullToZero : BaseNumericFunction
{
    protected override object EvaluateNull() => 0;
    protected override decimal? EvaluateNumeric(decimal numeric) => numeric;
}

[Scope("numeric/rounding")]
public abstract class BaseNumericRounding : BaseNumericFunction
{ }

/// <summary>
/// Returns the smallest integer greater than or equal to the argument number.
/// </summary>
public class Ceiling : BaseNumericRounding
{
    protected override decimal? EvaluateNumeric(decimal numeric) => Math.Ceiling(numeric);
}

/// <summary>
/// Returns the largest integer less than or equal to the argument number.
/// </summary>
public class Floor : BaseNumericRounding
{
    protected override decimal? EvaluateNumeric(decimal numeric) => Math.Floor(numeric);
}

/// <summary>
/// Returns the value of an argument number rounded to the nearest integer.
/// </summary>
public class Integer : BaseNumericRounding
{
    protected override decimal? EvaluateNumeric(decimal numeric) => Math.Round(numeric, 0);
}

/// <summary>
/// Returns the value of an argument number to the specified number of fractional digits.
/// </summary>
public class Round : BaseNumericRounding
{
    public Func<int> Digits { get; }

    /// <param name="digits">An integer between 0 and +Infinity, indicating the number of fractional digits in the return value.</param>
    public Round(Func<int> digits)
        => Digits = digits;

    protected override decimal? EvaluateNumeric(decimal numeric) => Math.Round(numeric, Digits.Invoke());
}

/// <summary>
/// Returns the value of an argument number, unless it is smaller than min, in which case it returns min, or greater than max, in which case it returns max.
/// </summary>
[Scope("numeric/rounding")]
public class Clip : BaseNumericFunction
{
    public Func<decimal> Min { get; }
    public Func<decimal> Max { get; }

    /// <param name="min">value returned in case the argument value is smaller than it.</param>
    /// <param name="max">value returned in case the argument value is greater than it.</param>
    public Clip(Func<decimal> min, Func<decimal> max)
        => (Min, Max) = (min, max);

    protected override decimal? EvaluateNumeric(decimal numeric)
        => (numeric < Min.Invoke()) ? Min.Invoke() : (numeric > Max.Invoke()) ? Max.Invoke() : numeric;
}

/// <summary>
/// Returns the reciprocal of the argument number, meaning the result of the division of 1 by the argument number. If the argument value is `0`, it returns `null`.
/// </summary>
[Scope("numeric/arithmetic")]
public class Invert : BaseNumericFunction
{
    public Invert()
    { }

    protected override decimal? EvaluateNumeric(decimal value) => value == 0 ? null : 1 / value;
}

/// <summary>
/// Returns the integer being the additive inverse of the argument meaning that their sum is equal to zero. The opposite of 0 is 0.
/// </summary>
[Scope("numeric/arithmetic")]
public class Oppose : BaseNumericFunction
{
    public Oppose()
    { }

    protected override decimal? EvaluateNumeric(decimal value) => value * -1;
}
