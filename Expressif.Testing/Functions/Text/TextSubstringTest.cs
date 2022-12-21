using Expressif.Functions.Text;
using Expressif.Values.Resolvers;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text
{
    public class TextSubstringTest
    {
        [Test]
        [TestCase("Foo+ Bar", "+ ", "Foo")]
        [TestCase("Foo+ Bar", "F", "")]
        [TestCase("Foo+ Bar", "B", "Foo+ ")]
        [TestCase("Foo+ Bar", "XYZ", "")]
        [TestCase("Foo+ Bar", "(null)", "")]
        [TestCase("Foo+ Bar", "(empty)", "")]
        [TestCase("(null)", ", ", "(null)")]
        [TestCase("(empty)", ", ", "(empty)")]
        public void BeforeSubstring_Valid(string value, string substring, string expected)
            => Assert.That(new BeforeSubstring(new LiteralScalarResolver<string>(substring)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("Foo+Bar+Bez+Quark", "+", 0, "Foo")]
        [TestCase("Foo+Bar+Bez+Quark", "+", 1, "Foo+Bar")]
        [TestCase("Foo+Bar+Bez+Quark", "+", 2, "Foo+Bar+Bez")]
        [TestCase("Foo+Bar+Bez+Quark", "+", 3, "(null)")]
        public void BeforeSubstring_Position_Valid(string value, string substring, int position, string expected)
            => Assert.That(new BeforeSubstring(new LiteralScalarResolver<string>(substring), new LiteralScalarResolver<int>(position)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("Foo+ Bar", "+ ", "Bar")]
        [TestCase("Foo+ Bar", "F", "oo+ Bar")]
        [TestCase("Foo+ Bar", "B", "ar")]
        [TestCase("Foo+ Bar", "XYZ", "")]
        [TestCase("Foo+ Bar", "(null)", "Foo+ Bar")]
        [TestCase("Foo+ Bar", "(empty)", "Foo+ Bar")]
        [TestCase("(null)", ", ", "(null)")]
        [TestCase("(empty)", ", ", "(empty)")]
        public void AfterSubstring_Valid(string value, string substring, string expected)
            => Assert.That(new AfterSubstring(new LiteralScalarResolver<string>(substring)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("Foo+Bar+Bez+Quark", "+", 0, "Bar+Bez+Quark")]
        [TestCase("Foo+Bar+Bez+Quark", "+", 1, "Bez+Quark")]
        [TestCase("Foo+Bar+Bez+Quark", "+", 2, "Quark")]
        [TestCase("Foo+Bar+Bez+Quark", "+", 3, "(null)")]
        public void AfterSubstring_Position_Valid(string value, string substring, int position, string expected)
            => Assert.That(new AfterSubstring(new LiteralScalarResolver<string>(substring), new LiteralScalarResolver<int>(position)).Evaluate(value)
                , Is.EqualTo(expected));
    }
}
