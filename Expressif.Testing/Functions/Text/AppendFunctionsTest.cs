using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text
{
    public class AppendFunctionsTest
    {
        [Test]
        [TestCase("123456789", "abc", "abc123456789")]
        [TestCase("(null)", "abc", "(null)")]
        [TestCase("(empty)", "abc", "abc")]
        [TestCase("(blank)", "abc", "abc")]
        public void Prefix_Valid(string value, string prefix, string expected)
            => Assert.That(new Prefix(() => prefix).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", "abc", "123456789abc")]
        [TestCase("(null)", "abc", "(null)")]
        [TestCase("(empty)", "abc", "abc")]
        [TestCase("(blank)", "abc", "abc")]
        public void Suffix_Valid(string value, string suffix, string expected)
            => Assert.That(new Suffix(() => suffix).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", 3, 2, "abc", "123abc6789")]
        [TestCase("123456789", 3, 5, "abc", "123abc9")]
        [TestCase("123456789", 3, 20, "abc", "123abc")]
        [TestCase("123456789", -3, 5, "abc", "abc3456789")]
        [TestCase("123456789", -3, 2, "abc", "abc123456789")]
        [TestCase("123456789", 8, -2, "abc", "123456abc9")]
        [TestCase("123456789", 20, -2, "abc", "123456789abc")]
        [TestCase("123456789", 5, -20, "abc", "abc6789")]
        [TestCase("(null)", 3, 2, "abc", "(null)")]
        [TestCase("(empty)", 3, 2, "abc", "abc")]
        [TestCase("(blank)", 3, 2, "abc", "abc")]
        public void ReplaceSlice_Valid(string value, int start, int length, string append, string expected)
            => Assert.That(new ReplaceSlice(() => start, () => length, () => append).Evaluate(value)
                , Is.EqualTo(expected));
    }
}
