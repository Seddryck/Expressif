using Expressif.Library.Numeric;

namespace Expressif.Library.Numeric.Arithmetic;

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
