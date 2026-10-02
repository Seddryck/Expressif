using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Library.Text;

namespace Expressif.Library.Numeric;

/// <summary>
/// Returns true if the numeric value passed as argument is a whole-number value. Returns `false` otherwise.
/// </summary>
[Predicate(aliases: ["is-integer", "integer", "numeric-is-integer"])]
public class WholeNumber : BaseNumericPredicate
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
