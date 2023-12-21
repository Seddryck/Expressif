using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Numeric;

/// <summary>
/// Returns an integer that indicates the sign of the argument value. It returns -1 if the value is strictly negative, 0 if the value is 0 and 1 if the value is strictly positive.
/// </summary>
public class Sign : BaseNumericFunction
{
    protected override decimal? EvaluateNumeric(decimal numeric) => Math.Sign(numeric);
}

/// <summary>
/// Returns the absolute value of the argument value.
/// </summary>
public class Absolute : BaseNumericFunction
{
    protected override decimal? EvaluateNumeric(decimal numeric) => Math.Abs(numeric);
}
