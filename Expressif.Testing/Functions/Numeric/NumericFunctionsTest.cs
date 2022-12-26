using Expressif.Functions.Numeric;
using Expressif.Functions.Text;
using Expressif.Values;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Expressif.Testing.Functions.Numeric
{
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
    }
}
