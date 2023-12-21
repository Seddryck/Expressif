using Expressif.Functions.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric;

public class PowerRootFunctionsTest
{
    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, 1)]
    [TestCase(2, 4)]
    [TestCase(-2, 4)]
    [TestCase(null, null)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void SquarePower_Valid(object? value, decimal? expected)
        => Assert.That(new SquarePower().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(2, 8)]
    [TestCase(-2, -8)]
    [TestCase(null, null)]
    public void CubePower_Valid(object? value, decimal? expected)
        => Assert.That(new CubePower().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0, 1)]
    [TestCase(1, 0, 1)]
    [TestCase(0, 1, 0)]
    [TestCase(1, 1, 1)]
    [TestCase(4, 1, 4)]
    [TestCase(-4, 1, -4)]
    [TestCase(0, 4, 0)]
    [TestCase(1, 4, 1)]
    [TestCase(-1, 4, 1)]
    [TestCase(2, 4, 16)]
    [TestCase(-2, 4, 16)]
    [TestCase(2, 5, 32)]
    [TestCase(-2, 5, -32)]
    [TestCase(null, 1, null)]
    public void Power_Exponent_Valid(object? value, decimal exponent, decimal? expected)
        => Assert.That(new Power(() => exponent).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(4, 2)]
    [TestCase(9, 3)]
    [TestCase(null, null)]
    public void SquareRoot_Valid(object? value, decimal? expected)
        => Assert.That(new SquareRoot().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(8, 2)]
    [TestCase(-8, -2)]
    [TestCase(null, null)]
    public void CubeRoot_Valid(object? value, decimal? expected)
        => Assert.That(new CubeRoot().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0, null)]
    [TestCase(1, 0, null)]
    [TestCase(0, 1, 0)]
    [TestCase(1, 1, 1)]
    [TestCase(4, 1, 4)]
    [TestCase(-4, 1, -4)]
    [TestCase(0, 4, 0)]
    [TestCase(1, 4, 1)]
    [TestCase(16, 4, 2)]
    [TestCase(-16, 4, null)]
    [TestCase(32, 5, 2)]
    [TestCase(-32, 5, -2)]
    [TestCase(null, 1, null)]
    public void NthRoot_Exponent_Valid(object? value, decimal exponent, decimal? expected)
        => Assert.That(new NthRoot(() => exponent).Evaluate(value), Is.EqualTo(expected));
}
