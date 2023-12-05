using Expressif.Predicates.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Numeric
{
    public class ComparisonShortcutTest
    {
        [Test]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(-10, false)]
        [TestCase(null, false)]
        public void Zero_Numeric_Success(object? value, bool expected)
            => Assert.That(new Zero().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(10, false)]
        [TestCase(-10, false)]
        [TestCase(null, false)]
        public void One_Numeric_Success(object? value, bool expected)
            => Assert.That(new One().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(-10, false)]
        [TestCase(null, false)]
        public void Positive_Numeric_Success(object? value, bool expected)
            => Assert.That(new Positive().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(-10, false)]
        [TestCase(null, false)]
        public void PositiveOrZero_Numeric_Success(object? value, bool expected)
            => Assert.That(new PositiveOrZero().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(-10, true)]
        [TestCase(null, false)]
        public void Negative_Numeric_Success(object? value, bool expected)
            => Assert.That(new Negative().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(-10, true)]
        [TestCase(null, false)]
        public void NegativeOrZero_Numeric_Success(object? value, bool expected)
            => Assert.That(new NegativeOrZero().Evaluate(value), Is.EqualTo(expected));
    }
}
