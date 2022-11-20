using Expressif.Predicates.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text
{
    internal class SubstringTest
    {
        [TestCase("Foobar", "Foo", true)]
        [TestCase("Foobar", "bar", false)]
        [TestCase("Foobar", "oba", false)]
        [TestCase("Foobar", "bing", false)]
        [TestCase("Foobar", "FoobarBaz", false)]
        [TestCase("", "(empty)", true)]
        [TestCase("Foobar", "(empty)", true)]
        [TestCase("(empty)", "", true)]
        [TestCase("(empty)", "Foo", false)]
        public void StartsWith_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new StartsWith(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("Foobar", "Foo", false)]
        [TestCase("Foobar", "bar", true)]
        [TestCase("Foobar", "oba", false)]
        [TestCase("Foobar", "bing", false)]
        [TestCase("Foobar", "FoobarBaz", false)]
        [TestCase("", "(empty)", true)]
        [TestCase("Foobar", "(empty)", true)]
        [TestCase("(empty)", "", true)]
        [TestCase("(empty)", "bar", false)]
        public void EndsWith_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new EndsWith(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("Foobar", "Foo", true)]
        [TestCase("Foobar", "bar", true)]
        [TestCase("Foobar", "oba", true)]
        [TestCase("Foobar", "bing", false)]
        [TestCase("Foobar", "FoobarBaz", false)]
        [TestCase("", "(empty)", true)]
        [TestCase("Foobar", "(empty)", true)]
        [TestCase("(empty)", "", true)]
        [TestCase("(empty)", "bar", false)]
        public void Contains_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new Expressif.Predicates.Text.Contains(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }
    }
}
