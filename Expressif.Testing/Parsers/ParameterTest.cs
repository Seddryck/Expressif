using Expressif.Parsers;
using Sprache;

namespace Expressif.Testing.Parsers
{
    public class ParameterTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase("foo", typeof(LiteralParameter))]
        [TestCase("\"foo\"", typeof(LiteralParameter))]
        [TestCase("@foo", typeof(VariableParameter))]
        [TestCase("[foo]", typeof(ObjectPropertyParameter))]
        [TestCase("#52", typeof(ObjectIndexParameter))]
        [TestCase("{ @foo | text-to-func(bar) }", typeof(InputExpressionParameter))]
        public void Parse_Parameter_Valid(string value, Type type)
            => Assert.That(Parameter.Parser.Parse(value), Is.TypeOf(type));

        [Test]
        [TestCase("(foo, bar)")]
        [TestCase("( \"foo\", bar ) ")]
        [TestCase("(@foo , bar)")]
        [TestCase("([foo] , #1)")]
        [TestCase("(@foo , { @foo | text-to-func(bar, @foo) })")]
        [TestCase("(@foo , { @foo | text-to-func(bar, { @fool | numeric-to-func(#3, [bez]) }) })")]
        public void Parse_Parameters_Valid(string value)
            => Assert.That(Parameters.Parser.Parse(value).Count, Is.EqualTo(2));
    }
}