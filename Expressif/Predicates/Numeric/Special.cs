using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric;

/// <summary>
/// Returns true if the numeric value passed as argument is an integer value. Returns `false` otherwise.
/// </summary>
public class Integer : BaseNumericPredicate
{
    protected override bool EvaluateNumeric(decimal value) => value % 1 == 0;
}

/// <summary>
/// Returns true if the numeric value passed as argument is equal to `0` or `null`. Returns `false` otherwise.
/// </summary>
public class ZeroOrNull : BaseNumericPredicate
{
    protected override bool EvaluateNull() => true;
    protected override bool EvaluateNumeric(decimal value) => value.Equals(0);
}
