using Expressif.Parsers;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers
{
    public class PredicationTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase("is-func(foo, @bar)", 1)]
        [TestCase("is-func", 1)]
        [TestCase("!is-func(foo)", 2)]
        [TestCase(" ! is-func(foo)", 2)]
        public void Parse_Predication_Valid(string value, int count)
            => Assert.That(Expressif.Parsers.Predication.Parser.Parse(value).Members, Has.Length.EqualTo(count));

        [Test]
        [TestCase("123 |? !equal-to(125)")]
        [TestCase("123 |? ! equal-to(125) ")]
        //[TestCase("123 |? !equal-to(125) |OR even ")]
        public void Parse_ParametrizedPredication_Valid(string value)
            => Assert.That(Expressif.Parsers.InputPredication.Parser.Parse(value).Member, Is.Not.Null);
    }
}