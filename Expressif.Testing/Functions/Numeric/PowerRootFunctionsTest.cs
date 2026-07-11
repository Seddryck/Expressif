using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class PowerRootFunctionsTest
{
    [Conformance]
    public void SquarePower_Valid(object? value, decimal? expected)
        => Assert.That(new SquarePower().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CubePower_Valid(object? value, decimal? expected)
        => Assert.That(new CubePower().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Power_Exponent_Valid(object? value, decimal exponent, decimal? expected)
        => Assert.That(new Power(() => exponent).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void SquareRoot_Valid(object? value, decimal? expected)
        => Assert.That(new SquareRoot().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CubeRoot_Valid(object? value, decimal? expected)
        => Assert.That(new CubeRoot().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NthRoot_Exponent_Valid(object? value, decimal exponent, decimal? expected)
        => Assert.That(new NthRoot(() => exponent).Evaluate(value), Is.EqualTo(expected));
}
