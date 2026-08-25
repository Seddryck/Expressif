using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric;

/// <summary>
/// Returns `true` if the division of the numeric value passed as argument by the modulus provided as parameter value is equal to the required remainder. Returns `false` otherwise.
/// </summary>
[Predicate(name: "has-remainder")]
public class Modulo : BaseNumericPredicateReference
{
    public Func<decimal> Remainder { get; }
    public Func<decimal> Modulus { get => Reference; }

    /// <param name="modulus">An integer value used as the modulus.</param>
    /// <param name="remainder">An integer value defined as the expected reminder.</param>
    public Modulo(Func<decimal> modulus, Func<decimal> remainder)
        : base(modulus) { Remainder = remainder; }

    protected override bool EvaluateNumeric(decimal value)
        => value % Modulus.Invoke() == Remainder.Invoke();
}

/// <summary>
/// Returns `true` if the numeric value passed as argument is evenly divisible by the divisor provided as parameter. Returns `false` otherwise.
/// </summary>
[Predicate]
public class DivisibleBy : Modulo
{
    public Func<decimal> Divisor { get => Modulus; }

    /// <param name="divisor">An integer value used as the divisor.</param>
    public DivisibleBy(Func<decimal> divisor)
        : base(divisor, () => 0) { }
}

/// <summary>
/// Returns `true` if the numeric value passed as argument is even. Returns `false` otherwise.
/// </summary>
[Predicate]
public class Even : Modulo
{
    public Even()
        : base(() => 2, () => 0) { }
}

/// <summary>
/// Returns `true` if the numeric value passed as argument is odd. Returns `false` otherwise.
/// </summary>
[Predicate]
public class Odd : Modulo
{
    public Odd()
        : base(() => 2, () => 1) { }

    protected override bool EvaluateNumeric(decimal value)
        => value % Modulus.Invoke() != 0;
}
