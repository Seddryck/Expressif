using Expressif.Predicates.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text
{
    public class EquivalentToTest
    {
        [TestCase("A", "A", true)]
        [TestCase("A", "B", false)]
        [TestCase("B", "A", false)]
        [TestCase("", "(empty)", true)]
        [TestCase("A", "(empty)", false)]
        [TestCase("(empty)", "", true)]
        [TestCase("(empty)", "A", false)]
        public void EquivalentTo_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new EquivalentTo(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("A", "A", false)]
        [TestCase("A", "B", false)]
        [TestCase("B", "A", true)]
        [TestCase("", "(empty)", false)]
        [TestCase("A", "(empty)", true)]
        [TestCase("(empty)", "", false)]
        [TestCase("(empty)", "A", false)]
        public void SortedAfter_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new SortedAfter(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("A", "A", true)]
        [TestCase("A", "B", false)]
        [TestCase("B", "A", true)]
        [TestCase("", "(empty)", true)]
        [TestCase("A", "(empty)", true)]
        [TestCase("(empty)", "", true)]
        [TestCase("(empty)", "A", false)]
        public void SortedAfterOrEquivalentTo_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new SortedAfterOrEquivalentTo(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("A", "A", false)]
        [TestCase("A", "B", true)]
        [TestCase("B", "A", false)]
        [TestCase("", "(empty)", false)]
        [TestCase("A", "(empty)", false)]
        [TestCase("(empty)", "", false)]
        [TestCase("(empty)", "A", true)]
        public void SortedBefore_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new SortedBefore(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }

        [TestCase("A", "A", true)]
        [TestCase("A", "B", true)]
        [TestCase("B", "A", false)]
        [TestCase("", "(empty)", true)]
        [TestCase("A", "(empty)", false)]
        [TestCase("(empty)", "", true)]
        [TestCase("(empty)", "A", true)]
        public void SortedBeforeOrEquivalentTo_Text_Success(object value, object reference, bool expected)
        {

            var predicate = new SortedBeforeOrEquivalentTo(new LiteralScalarResolver<string>(reference));
            Assert.Multiple(() =>
            {
                Assert.That(predicate.Reference.Execute(), Is.EqualTo(reference));
                Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
            });
        }
    }
}
