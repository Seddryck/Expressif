using Expressif.Predicates.Numeric;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Numeric
{
    public class ModuloTest
    {
        [Test]
        [TestCase(10, 5, 0)]
        [TestCase(10, 4, 2)]
        public void Modulo_Numeric_Success(object value, int modulus, int remainder)
        {
            var predicate = new Modulo(() => modulus, () => remainder);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(modulus));
                Assert.That(predicate.Remainder.Invoke(), Is.EqualTo(remainder));
                Assert.That(predicate.Evaluate(value), Is.True);
            });
        }

        [Test]
        [TestCase(10, 6, 0)]
        [TestCase(10, 5, 1)]
        [TestCase(null, 5, 2)]
        public void Modulo_Numeric_Failure(object value, int modulus, int remainder)
        {
            var predicate = new Modulo(() => modulus, () => remainder);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(modulus));
                Assert.That(predicate.Remainder.Invoke(), Is.EqualTo(remainder));
                Assert.That(predicate.Evaluate(value), Is.False);
            });
        }

        //[Test]
        //[TestCase(5, null, 2)]
        //[TestCase(5, 5, null)]
        //public void Modulo_Numeric_Exception(object value, int modulus, int remainder)
        //    => Assert.That(() => new Modulo(
        //            () => modulus
        //            , () => remainder
        //        ).Evaluate(value)
        //    , Throws.TypeOf<NullReferenceException>());

        [Test]
        [TestCase(10, true)]
        [TestCase(1, false)]
        [TestCase(0, true)]
        [TestCase(null, false)]
        public void Even_Numeric_Valid(object value, bool expected)
        {
            var predicate = new Even();
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(10, false)]
        [TestCase(1, true)]
        [TestCase(0, false)]
        [TestCase(null, false)]
        public void Odd_Numeric_Valid(object value, bool expected)
        {
            var predicate = new Odd();
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        }
    }
}
