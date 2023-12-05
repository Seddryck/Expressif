using Expressif.Predicates.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Numeric
{
    public class SpecialTest
    {
        [Test]
        [TestCase(0, true)]
        [TestCase(4, true)]
        [TestCase("4.0", true)]
        [TestCase(4.25, false)]
        [TestCase(null, false)]
        [TestCase("(null)", false)]
        public void Integer_Numeric_Success(object? value, bool expected)
            => Assert.That(new Integer().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, true)]
        [TestCase(4, false)]
        [TestCase("4.0", false)]
        [TestCase(4.25, false)]
        [TestCase(null, true)]
        [TestCase("(null)", true)]
        public void ZeroOrNull_Numeric_Success(object? value, bool expected)
            => Assert.That(new ZeroOrNull().Evaluate(value), Is.EqualTo(expected));
    }
}
