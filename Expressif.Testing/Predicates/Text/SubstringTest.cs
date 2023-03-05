using Expressif.Predicates.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text
{
    public class SubstringTest
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
        public void StartsWith_Text_Success(object value, string reference, bool expected)
        {
            var predicate = new StartsWith(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
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
        public void EndsWith_Text_Success(object value, string reference, bool expected)
        {
            var predicate = new EndsWith(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
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
        public void Contains_Text_Success(object value, string reference, bool expected)
        {
            var predicate = new Expressif.Predicates.Text.Contains(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("FooBar", "^[A-Z][a-z]+$", true)]
        [TestCase("FooBar", "^[A-Z]+$", true)]
        [TestCase("FOOBAR", "^[A-Z]+$", true)]
        [TestCase("(empty)", "^[A-Z]+$", false)]
        [TestCase("", "^[A-Z]+$", false)]
        [TestCase("(null)", "^[A-Z]+$", false)]
        [TestCase(null, "^[A-Z]+$", false)]
        public void MatchesRegex_TextIgnoreCase_Success(object value, string reference, bool expected)
        {
            var predicate = new MatchesRegex(() => reference);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("FooBar", "^[A-Z][a-z]+$", false)]
        [TestCase("FOOBAR", "^[A-Z][a-z]+$", false)]
        [TestCase("foobar", "^[A-Z][a-z]+$", false)]
        [TestCase("FooBar", "^[A-Z]+$", false)]
        [TestCase("FOOBAR", "^[A-Z]+$", true)]
        [TestCase("FOOBAR", "^[a-z]+$", false)]
        public void MatchesRegex_TextDontIgnoreCase_Success(object value, string reference, bool expected)
        {
            var predicate = new MatchesRegex(() => reference, StringComparer.InvariantCulture);
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }
    }
}
