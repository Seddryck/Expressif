using Expressif.Functions.Numeric;
using Expressif.Functions.Text;
using Expressif.Values;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Expressif.Testing.Functions.Numeric
{
    [TestFixture]
    public class NumericTransformationsTest
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
        [TestCase(0.5, 0)]
        [TestCase(0.9999, 0)]
        [TestCase(-0.5, -1)]
        [TestCase(-0.9999, -1)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToFloor_Valid(object value, decimal? expected)
            => Assert.That(new NumericToFloor().Evaluate(value), Is.EqualTo(expected));

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
        public void NumericToCeiling_Valid(object value, decimal? expected)
            => Assert.That(new NumericToCeiling().Evaluate(value), Is.EqualTo(expected));

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
        public void NumericToInteger_Valid(object value, decimal? expected)
            => Assert.That(new NumericToInteger().Evaluate(value), Is.EqualTo(expected));

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
        public void NumericToRound_Valid(object value, decimal? expected)
            => Assert.That(new NumericToRound(new LiteralScalarResolver<int>(2)).Evaluate(value), Is.EqualTo(expected));

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
        public void NumericToClip_Interval1010Valid(object value, decimal? expected)
            => Assert.That(new NumericToClip(
                    new LiteralScalarResolver<decimal>(-10)
                    , new LiteralScalarResolver<decimal>(10))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(-1, 0)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToIncrement_Valid(object value, decimal? expected)
            => Assert.That(new NumericToIncrement().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, -1)]
        [TestCase(1, 0)]
        [TestCase(-1, -2)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToDecrement_Valid(object value, decimal? expected)
            => Assert.That(new NumericToDecrement().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, 3)]
        [TestCase(1, 4)]
        [TestCase(-1, 2)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToAdd_3_Valid(object value, decimal? expected)
            => Assert.That(new NumericToAdd(new LiteralScalarResolver<decimal>(3)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, 6)]
        [TestCase(1, 7)]
        [TestCase(-1, 5)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToAdd_3Times2_Valid(object value, decimal? expected)
            => Assert.That(new NumericToAdd(
                    new LiteralScalarResolver<decimal>(3)
                    , new LiteralScalarResolver<int>(2))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, -3)]
        [TestCase(1, -2)]
        [TestCase(-1, -4)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToSubtract_3_Valid(object value, decimal? expected)
            => Assert.That(new NumericToSubtract(new LiteralScalarResolver<decimal>(3)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, -6)]
        [TestCase(1, -5)]
        [TestCase(-1, -7)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToSubtract_3Times2_Valid(object value, decimal? expected)
            => Assert.That(new NumericToSubtract(
                    new LiteralScalarResolver<decimal>(3)
                    , new LiteralScalarResolver<int>(2))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 3)]
        [TestCase(-1, -3)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToMultiply_3_Valid(object value, decimal? expected)
            => Assert.That(new NumericToMultiply(
                    new LiteralScalarResolver<decimal>(3))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 0.25)]
        [TestCase(-1, -0.25)]
        [TestCase("(null)", null)]
        [TestCase("(empty)", null)]
        [TestCase("(blank)", null)]
        public void NumericToDivide_4_Valid(object value, decimal? expected)
            => Assert.That(new NumericToDivide(
                    new LiteralScalarResolver<decimal>(4))
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
        public void NumericToInvert_Valid(object value, decimal? expected)
            => Assert.That(new NumericToInvert().Evaluate(value), Is.EqualTo(expected));
    }
}
