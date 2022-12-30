using Expressif.Predicates.Boolean;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Boolean
{
    public class IdenticalToTest
    {
        [Test]
        [TestCase(true, true, true)]
        [TestCase(false, true, false)]
        [TestCase(null, true, false)]
        [TestCase("(null)", true, false)]
        [TestCase(-1, true, true)]
        [TestCase(0, true, false)]
        [TestCase(1, true, true)]
        [TestCase(104.25, true, true)]
        [TestCase("true", true, true)]
        [TestCase("false", true, false)]
        [TestCase("yes", true, true)]
        [TestCase("no", true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, false, true)]
        [TestCase(null, false, false)]
        [TestCase("(null)", false, false)]
        [TestCase(-1, false, false)]
        [TestCase(0, false, true)]
        [TestCase(1, false, false)]
        [TestCase(104.25, false, false)]
        [TestCase("true", false, false)]
        [TestCase("false", false, true)]
        [TestCase("yes", false, false)]
        [TestCase("no", false, true)]
        public void False_Boolean_Expected(object value, bool reference, bool expected)
            => Assert.That(new IdenticalTo(() => reference).Evaluate(value), Is.EqualTo(expected));
    }
}
