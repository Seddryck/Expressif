using Expressif.Parsers;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers
{
    public class PredicateExpressionTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase("is-func(foo, @bar)", 1)]
        [TestCase("is-func", 1)]
        [TestCase("!is-func(foo)", 2)]
        [TestCase(" ! is-func(foo)", 2)]
        public void Parse_Expression_Valid(string value, int count)
        {
            var predication = Expressif.Parsers.Predication.Parser.Parse(value);
            Assert.That(predication.Members, Is.Not.Null);
            Assert.That(predication.Members, Has.Length.EqualTo(count));
        }
    }
}