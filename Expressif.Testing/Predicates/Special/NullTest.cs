using Expressif.Predicates.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Special;

public class NullTest
{
    [Test]
    [TestCase(null, true)]
    [TestCase(0, false)]
    [TestCase(4, false)]
    [TestCase("(null)", true)]
    [TestCase("", false)]
    [TestCase("(empty)", false)]
    [TestCase("(blank)", false)]
    [TestCase("foo", false)]
    public void Null_Numeric_Success(object? value, bool expected)
    => Assert.That(new Null().Evaluate(value), Is.EqualTo(expected));
}
