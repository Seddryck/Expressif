using Expressif.Predicates.DateTime;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.DateTime
{
    public class ComparisonTest
    {
        [Test]
        [TestCase("2022-11-21", "2022-11-21", true)]
        [TestCase("2022-11-21", "2022-11-22", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", false)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void SameInstant_DateTime_Expected(object value, object reference, bool expected)
            => Assert.That(new SameInstant(new LiteralScalarResolver<System.DateTime>(reference)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", false)]
        [TestCase("2022-11-21", "2022-11-22", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", true)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void After_DateTime_Expected(object value, object reference, bool expected)
            => Assert.That(new After(new LiteralScalarResolver<System.DateTime>(reference)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", true)]
        [TestCase("2022-11-21", "2022-11-22", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", true)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void AfterOrSameInstant_DateTime_Expected(object value, object reference, bool expected)
            => Assert.That(new AfterOrSameInstant(new LiteralScalarResolver<System.DateTime>(reference)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", false)]
        [TestCase("2022-11-21", "2022-11-22", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", false)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void Before_DateTime_Expected(object value, object reference, bool expected)
            => Assert.That(new Before(new LiteralScalarResolver<System.DateTime>(reference)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", true)]
        [TestCase("2022-11-21", "2022-11-22", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", false)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void BeforeOrSameInstant_DateTime_Expected(object value, object reference, bool expected)
            => Assert.That(new BeforeOrSameInstant(new LiteralScalarResolver<System.DateTime>(reference)).Evaluate(value), Is.EqualTo(expected));
    }
}
