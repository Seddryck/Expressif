using Expressif.Functions.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric
{
    public class SignFunctionsTest
    {
        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(-1, -1)]
        [TestCase(5, 1)]
        [TestCase(-5, -1)]
        [TestCase(null, null)]
        public void Sign_Valid(object? value, decimal? expected)
            => Assert.That(new Sign().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(-1, 1)]
        [TestCase(5, 5)]
        [TestCase(-5, 5)]
        [TestCase(null, null)]
        public void Absolute_Valid(object? value, decimal? expected)
            => Assert.That(new Absolute().Evaluate(value), Is.EqualTo(expected));
    }
}
