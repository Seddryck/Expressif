using Expressif.Parsers;
using Expressif.Values;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers
{
    public class GrammarTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("foo")]
        [TestCase("foo  ")]
        [TestCase("  foo")]
        [TestCase("FoO")]
        [TestCase("foo-bar")]
        [TestCase("  foo-bar")]
        [TestCase("foo-bar  ")]
        [TestCase("foo-BAr-foo")]
        public void Parse_FunctionName_Valid(string value)
            => Assert.That(Grammar.FunctionName.End().Parse(value), Is.EqualTo(value.Trim()));

        [Test]
        [TestCase("1foo")]
        [TestCase("fo1o")]
        [TestCase("foo1")]
        [TestCase("@foo")]
        [TestCase("-foo")]
        [TestCase("foo--bar")]
        [TestCase("foo-")]
        public void Parse_FunctionName_Invalid(string value)
            => Assert.That(() => Grammar.FunctionName.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("|")]
        [TestCase("  |")]
        [TestCase("|  ")]
        public void Parse_Delimitator_Valid(string value)
            => Assert.That(Grammar.Delimitator.End().Parse(value), Is.EqualTo('|'));

        [Test]
        [TestCase("@")]
        [TestCase("foo")]
        public void Parse_Delimitator_Invalid(string value)
            => Assert.That(() => Grammar.Delimitator.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("@a")]
        [TestCase("@foo")]
        [TestCase("@foo1")]
        [TestCase("@fo1o")]
        [TestCase("@Foo  ")]
        [TestCase("  @Foo")]
        public void Parse_Variable_Valid(string value)
            => Assert.That(Grammar.Variable.End().Parse(value), Is.EqualTo(value.Trim().TrimStart('@')));

        [Test]
        [TestCase("@")]
        [TestCase("foo")]
        [TestCase("@foo-1")]
        [TestCase("@ foo")]
        [TestCase("@1foo")]
        public void Parse_Variable_Invalid(string value)
            => Assert.That(() => Grammar.Delimitator.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("foo")]
        [TestCase("  foo")]
        [TestCase("foo  ")]
        [TestCase("foo-bar")]
        [TestCase("%foo!bar")]
        [TestCase("\"foo\"")]
        [TestCase("\" foo bar \"")]
        [TestCase("\"foo , bar\"")]
        [TestCase("\"(foo)\"")]
        public void Parse_Literal_Valid(string value)
            => Assert.That(Grammar.Literal.End().Parse(value), Is.EqualTo(value.Trim().Trim('\"')));

        [Test]
        [TestCase("@foo")]
        [TestCase("foo bar")]
        [TestCase("foo , bar")]
        [TestCase("(foo)")]
        public void Parse_Literal_Invalid(string value)
            => Assert.That(() => Grammar.Literal.End().Parse(value), Throws.TypeOf<ParseException>());
    }
}