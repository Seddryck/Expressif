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
    public class TextPositionTest
    {
        [Test]
        [TestCase("Foo, Bar", ", ", "Foo")]
        [TestCase("Foo, Bar", "F", "")]
        [TestCase("Foo, Bar", "B", "Foo, ")]
        [TestCase("Foo, Bar", "XYZ", "")]
        [TestCase("Foo, Bar", "(null)", "")]
        [TestCase("Foo, Bar", "(empty)", "")]
        [TestCase("(null)", ", ", "(null)")]
        [TestCase("(empty)", ", ", "(empty)")]
        public void TextToBefore_Valid(string value, string substring, string expected)
            => Assert.That(new TextToBefore(new LiteralScalarResolver<string>(substring)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        //[TestCase("Foo, Bar", ", ", "Bar")]
        //[TestCase("Foo, Bar", "F", "oo, Bar")]
        //[TestCase("Foo, Bar", "B", "ar")]
        //[TestCase("Foo, Bar", "XYZ", "")]
        //[TestCase("Foo, Bar", "(null)", "Foo, Bar")]
        [TestCase("Foo, Bar", "(empty)", "Foo, Bar")]
        //[TestCase("(null)", ", ", "(null)")]
        //[TestCase("(empty)", ", ", "(empty)")]
        public void TextToAfter_Valid(string value, string substring, string expected)
            => Assert.That(new TextToAfter(new LiteralScalarResolver<string>(substring)).Evaluate(value)
                , Is.EqualTo(expected));
    }
}
