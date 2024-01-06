using Expressif.Functions.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class NumericFunctionsTest
{
    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(0.9999, 0.9999)]
    [TestCase("(null)", 0)]
    [TestCase("(empty)", 0)]
    [TestCase("(blank)", 0)]
    public void NullToZero_Valid(object value, decimal? expected)
        => Assert.That(new NullToZero().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(0.9999, 0.9999)]
    public void NullToZero_Valid(decimal value, decimal? expected)
        => Assert.That(new NullToZero().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(typeof(DBNull), 0)]
    public void NullToZero_DBNull_Valid(Type type, decimal? expected)
        => Assert.That(new NullToZero().Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(0.5, 0)]
    [TestCase(0.9999, 0)]
    [TestCase(-0.5, -1)]
    [TestCase(-0.9999, -1)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Floor_Valid(object value, decimal? expected)
        => Assert.That(new Floor().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(0.5, 1)]
    [TestCase(0.9999, 1)]
    [TestCase(-0.5, 0)]
    [TestCase(-0.9999, 0)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Ceiling_Valid(object value, decimal? expected)
        => Assert.That(new Ceiling().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(0.0001, 0)]
    [TestCase(0.5, 0)]
    [TestCase(0.9999, 1)]
    [TestCase(-0.0001, 0)]
    [TestCase(-0.5, 0)]
    [TestCase(-0.9999, -1)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Integer_Valid(object value, decimal? expected)
        => Assert.That(new Integer().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(0.0001, 0)]
    [TestCase(0.489, 0.49)]
    [TestCase(0.491, 0.49)]
    [TestCase(0.5, 0.5)]
    [TestCase(0.501, 0.5)]
    [TestCase(0.9999, 1)]
    [TestCase(-0.0001, 0)]
    [TestCase(-0.5, -0.5)]
    [TestCase(-0.9999, -1)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Round_Valid(object value, decimal? expected)
        => Assert.That(new Round(() => 2).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(10, 10)]
    [TestCase(-10, -10)]
    [TestCase(15, 10)]
    [TestCase(-15, -10)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Clip_Interval1010Valid(object value, decimal? expected)
        => Assert.That(new Clip(() => -10, () => 10)
            .Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, null)]
    [TestCase(1, 1)]
    [TestCase(-1, -1)]
    [TestCase(4, 0.25)]
    [TestCase(-4, -0.25)]
    [TestCase(0.25, 4)]
    [TestCase(-0.25, -4)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Invert_Valid(object value, decimal? expected)
        => Assert.That(new Invert().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, -1)]
    [TestCase(-1, 1)]
    [TestCase(4, -4)]
    [TestCase(-4, 4)]
    [TestCase(null, null)]
    [TestCase("(null)", null)]
    [TestCase("(empty)", null)]
    [TestCase("(blank)", null)]
    public void Oppose_Valid(object? value, decimal? expected)
        => Assert.That(new Oppose().Evaluate(value), Is.EqualTo(expected));
}
