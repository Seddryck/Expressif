using Expressif.Predicates.Boolean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Boolean
{
    public class TrueFalseTest
    {
        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(null, false)]
        [TestCase("(null)", false)]
        [TestCase(-1, true)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(104.25, true)]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("yes", true)]
        [TestCase("no", false)]
        public void True_Boolean_Expected(object value, bool expected)
            => Assert.That(new True().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(null, true)]
        [TestCase("(null)", true)]
        [TestCase(-1, true)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(104.25, true)]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("yes", true)]
        [TestCase("no", false)]
        public void TrueOrNull_Boolean_Expected(object value, bool expected)
            => Assert.That(new TrueOrNull().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(null, false)]
        [TestCase("(null)", false)]
        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(104.25, false)]
        [TestCase("true", false)]
        [TestCase("false", true)]
        [TestCase("yes", false)]
        [TestCase("no", true)]
        public void False_Boolean_Expected(object value, bool expected)
            => Assert.That(new False().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(null, true)]
        [TestCase("(null)", true)]
        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(104.25, false)]
        [TestCase("true", false)]
        [TestCase("false", true)]
        [TestCase("yes", false)]
        [TestCase("no", true)]
        public void FalseOrNull_Boolean_Expected(object value, bool expected)
            => Assert.That(new FalseOrNull().Evaluate(value), Is.EqualTo(expected));
    }
}
