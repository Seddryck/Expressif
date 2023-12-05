using Expressif.Predicates.Temporal;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Temporal
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
        public void SameInstant_DateTime_Expected(object? value, DateTime reference, bool expected)
            => Assert.That(new SameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", false)]
        [TestCase("2022-11-21", "2022-11-22", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", true)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void After_DateTime_Expected(object? value, DateTime reference, bool expected)
            => Assert.That(new After(() => reference).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", true)]
        [TestCase("2022-11-21", "2022-11-22", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", true)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void AfterOrSameInstant_DateTime_Expected(object? value, DateTime reference, bool expected)
            => Assert.That(new AfterOrSameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", false)]
        [TestCase("2022-11-21", "2022-11-22", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", false)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", false)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void Before_DateTime_Expected(object? value, DateTime reference, bool expected)
            => Assert.That(new Before(() => reference).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", "2022-11-21", true)]
        [TestCase("2022-11-21", "2022-11-22", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:25", true)]
        [TestCase("2022-11-21 17:12:25", "2022-11-21 17:12:24", false)]
        [TestCase(null, "2022-11-21", false)]
        [TestCase("(null)", "2022-11-21", false)]
        public void BeforeOrSameInstant_DateTime_Expected(object? value, DateTime reference, bool expected)
            => Assert.That(new BeforeOrSameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));
    }
}
