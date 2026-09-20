using Expressif.Library.Numeric.Arithmetic;
using Expressif.Library.Numeric.Classification;
using Expressif.Library.Numeric.Conversion;
using Expressif.Library.Numeric.Formatting;
using Expressif.Library.Numeric.Rounding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Numeric.Arithmetic;

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
    public void Power_Valid_Exponent(object? value, decimal exponent, decimal? expected)
        => Assert.That(new Power(() => exponent).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void SquareRoot_Valid(object? value, decimal? expected)
        => Assert.That(new SquareRoot().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CubeRoot_Valid(object? value, decimal? expected)
        => Assert.That(new CubeRoot().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NthRoot_Valid_Exponent(object? value, decimal exponent, decimal? expected)
        => Assert.That(new NthRoot(() => exponent).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NthRoot_Invalid(object? value, decimal exponent, decimal? expected)
        => Assert.That(new NthRoot(() => exponent).Evaluate(value), Is.EqualTo(expected));
}
