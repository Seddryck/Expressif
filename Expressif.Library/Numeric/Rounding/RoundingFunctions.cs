using System;
using Expressif.Library.Numeric;

namespace Expressif.Library.Numeric.Rounding;

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
