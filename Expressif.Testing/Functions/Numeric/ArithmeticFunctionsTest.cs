using Expressif.Functions.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric;

public class ArithmeticFunctionsTest
{
    [Test]
    [TestCase(0, 1)]
    [TestCase(1, 2)]
    [TestCase(-1, 0)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Increment_Valid(object value, decimal? expected)
        => Assert.That(new Increment().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, -1)]
    [TestCase(1, 0)]
    [TestCase(-1, -2)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Decrement_Valid(object value, decimal? expected)
        => Assert.That(new Decrement().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 3)]
    [TestCase(1, 4)]
    [TestCase(-1, 2)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Add_3_Valid(object value, decimal? expected)
        => Assert.That(new Add(() => 3).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 6)]
    [TestCase(1, 7)]
    [TestCase(-1, 5)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Add_3Times2_Valid(object value, decimal? expected)
        => Assert.That(new Add(() => 3, () => 2)
            .Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, -3)]
    [TestCase(1, -2)]
    [TestCase(-1, -4)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Subtract_3_Valid(object value, decimal? expected)
        => Assert.That(new Subtract(() => 3).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, -6)]
    [TestCase(1, -5)]
    [TestCase(-1, -7)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Subtract_3Times2_Valid(object value, decimal? expected)
        => Assert.That(new Subtract(() => 3, () => 2)
            .Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 3)]
    [TestCase(-1, -3)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Multiply_3_Valid(object value, decimal? expected)
        => Assert.That(new Multiply(() => 3)
            .Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 0.25)]
    [TestCase(-1, -0.25)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Divide_4_Valid(object value, decimal? expected)
        => Assert.That(new Divide(() => 4)
            .Evaluate(value), Is.EqualTo(expected));
}
