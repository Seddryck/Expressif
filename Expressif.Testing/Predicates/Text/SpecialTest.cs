using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text
{
    public class SpecialTest
    {
        [TestCase(null, true)]
        [TestCase("(null)", true)]
        [TestCase("", false)]
        [TestCase("(empty)", false)]
        [TestCase("(blank)", false)]
        [TestCase("foo", false)]
        public void Null_Text_Success(object value, bool expected)
            => Assert.That(new Null().Evaluate(value), Is.EqualTo(expected));

        [TestCase(null, false)]
        [TestCase("(null)", false)]
        [TestCase("", true)]
        [TestCase("(empty)", true)]
        [TestCase("(blank)", false)]
        [TestCase("foo", false)]
        public void Empty_Text_Success(object value, bool expected)
            => Assert.That(new Empty().Evaluate(value), Is.EqualTo(expected));
    }
}
