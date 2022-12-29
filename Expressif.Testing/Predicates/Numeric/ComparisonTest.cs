using Expressif.Predicates.Numeric;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Numeric
{
    public class ComparisonTest
    {
        [Test]
        [TestCase(10, 10, true)]
        [TestCase(10, 4, false)]
        [TestCase(10, 15, false)]
        [TestCase(4.001, 4, false)]
        [TestCase(3.999, 4, false)]
        [TestCase(null, 4, false)]
        public void Equal_Numeric_Success(object value, decimal reference, bool expected)
        {
            var predicate = new EqualTo(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase(10, 10, false)]
        [TestCase(10, 4, true)]
        [TestCase(10, 15, false)]
        [TestCase(4.001, 4, true)]
        [TestCase(3.999, 4, false)]
        [TestCase(null, 4, false)]
        public void GreaterThan_Numeric_Success(object value, decimal reference, bool expected)
        {
            var predicate = new GreaterThan(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase(10, 10, true)]
        [TestCase(10, 4, true)]
        [TestCase(10, 15, false)]
        [TestCase(4.001, 4, true)]
        [TestCase(3.999, 4, false)]
        [TestCase(null, 4, false)]
        public void GreaterThanOrEqual_Numeric_Success(object value, decimal reference, bool expected)
        {
            var predicate = new GreaterThanOrEqual(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase(10, 10, false)]
        [TestCase(10, 4, false)]
        [TestCase(10, 15, true)]
        [TestCase(4.001, 4, false)]
        [TestCase(3.999, 4, true)]
        [TestCase(null, 4, false)]
        public void LessThan_Numeric_Success(object value, decimal reference, bool expected)
        {
            var predicate = new LessThan(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase(10, 10, true)]
        [TestCase(10, 4, false)]
        [TestCase(10, 15, true)]
        [TestCase(4.001, 4, false)]
        [TestCase(3.999, 4, true)]
        [TestCase(null, 4, false)]
        public void LessThanOrEqual_Numeric_Success(object value, decimal reference, bool expected)
        {
            var predicate = new LessThanOrEqual(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }


        [Test]
        [TestCase(10, -10, true)]
        [TestCase(10, -11, false)]
        [TestCase(0, 0, true)]
        [TestCase(10, 10, false)]
        [TestCase(-4, 4, true)]
        [TestCase(-4, 5, false)]
        [TestCase(-4, -4, false)]
        [TestCase(null, 4, false)]
        public void Opposite_Numeric_Success(object value, decimal reference, bool expected)
        {
            var predicate = new Opposite(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }
    }
}
