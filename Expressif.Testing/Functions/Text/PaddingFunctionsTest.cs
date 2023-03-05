using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text
{
    public class PaddingFunctionsTest
    {
        [Test]
        [TestCase("1234", 9, '0', "123400000")]
        [TestCase(1234, 9, '0', "123400000")]
        [TestCase("1234", 9, '*', "1234*****")]
        [TestCase(1234, 9, '*', "1234*****")]
        [TestCase("123456789", 3, '0', "123456789")]
        [TestCase("(null)", 3, '0', "000")]
        [TestCase("(empty)", 3, '0', "000")]
        public void PadRight_Valid(object value, int length, char character, string expected)
            => Assert.That(new PadRight(() => (length), () => (character))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("1234", 9, '0', "000001234")]
        [TestCase("1234", 9, '*', "*****1234")]
        [TestCase("123456789", 3, '0', "123456789")]
        [TestCase("(null)", 3, '0', "000")]
        [TestCase("(empty)", 3, '0', "000")]
        public void PadLeft_Valid(string value, int length, char character, string expected)
            => Assert.That(new PadLeft(() => (length), () => (character))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("1234", 4, '*', "1234")]
        [TestCase("1234", 5, '*', "1234*")]
        [TestCase("1234", 6, '*', "*1234*")]
        [TestCase("1234", 7, '*', "*1234**")]
        [TestCase("1234", 8, '*', "**1234**")]
        [TestCase("1234", 9, '*', "**1234***")]
        [TestCase("1234", 2, '*', "1234")]
        [TestCase("(null)", 3, '0', "000")]
        [TestCase("(empty)", 3, '0', "000")]
        public void PadCenter_Valid(string value, int length, char character, string expected)
            => Assert.That(new PadCenter(() => (length), () => (character))
                .Evaluate(value), Is.EqualTo(expected));
    }
}
