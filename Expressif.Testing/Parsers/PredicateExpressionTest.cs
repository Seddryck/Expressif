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
        //[TestCase("is-func(foo) | numeric-to-func(foo, @bar)", 2)]
        //[TestCase("text-to-func(foo) | numeric-to-func(foo, @bar) | boolean-to-func", 3)]
        public void Parse_Expression_Valid(string value, int count)
            => Assert.That(PredicateExpression.Parser.Parse(value).Member, Is.Not.Null);
        
        //[Test]
        //[TestCase("@foo | text-to-func(foo, @bar)", 1)]
        //[TestCase("@foo | text-to-func(foo) | numeric-to-func(foo, @bar)", 2)]
        //[TestCase("foo", 0)]
        //public void Parse_ParametrizedExpression_Valid(string value, int count)
        //    => Assert.That(InputExpression.Parser.Parse(value).Members.Count, Is.EqualTo(count));
    }
}