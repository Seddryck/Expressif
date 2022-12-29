using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text
{
    public class CharFunctionsTest
    {

        [Test]
        [TestCase("123*456*78", "*", "12345678")]
        [TestCase("***123***456*78****", "*", "12345678")]
        [TestCase("******", "*", "")]
        [TestCase("(null)", "*", "(null)")]
        [TestCase("(empty)", "*", "(empty)")]
        [TestCase("(blank)", "*", "(blank)")]
        [TestCase("(blank)", " ", "(empty)")]
        public void RemoveChars_Valid(string value, char charToRemove, string expected)
            => Assert.That(new RemoveChars(() => (charToRemove)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123*456*78", "*", "-", "123-456-78")]
        [TestCase("***123***456*78****", "*", "-", "---123---456-78----")]
        [TestCase("******", "*", "-", "------")]
        [TestCase("(null)", "*", "-", "(null)")]
        [TestCase("(empty)", "*", "-", "(empty)")]
        [TestCase("(blank)", "*", "-", "(blank)")]
        [TestCase("(blank)", " ", "-", "(blank)")]
        public void ReplaceChars_Valid(string value, char charToReplace, char replacingChar, string expected)
            => Assert.That(new ReplaceChars(() => charToReplace, () => replacingChar).Evaluate(value)
                , Is.EqualTo(expected));
    }
}
