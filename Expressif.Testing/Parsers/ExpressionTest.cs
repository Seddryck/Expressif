using Expressif.Parsers;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers;

public class ExpressionTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("text-to-func(foo, @bar)", 1)]
    [TestCase("text-to-func", 1)]
    [TestCase("text-to-func(foo) | numeric-to-func(foo, @bar)", 2)]
    [TestCase("text-to-func(foo) | numeric-to-func(foo, @bar) | boolean-to-func", 3)]
    public void Parse_Expression_Valid(string value, int count)
        => Assert.That(Expressif.Parsers.Expression.Parser.Parse(value).Members.Count, Is.EqualTo(count));

    [Test]
    [TestCase("@foo | text-to-func(foo, @bar)", 1)]
    [TestCase("@foo | text-to-func(foo) | numeric-to-func(foo, @bar)", 2)]
    [TestCase("foo", 0)]
    public void Parse_ParametrizedExpression_Valid(string value, int count)
        => Assert.That(InputExpression.Parser.Parse(value).Members.Count, Is.EqualTo(count));

    [Test]
    [TestCase("{1,2,3} | sum")]
    [TestCase("@foo | count")]
    [TestCase("[foo] | min")]
    [TestCase("#1 | last")]
    public void Parse_InputExpression_ImplicitFoldAggregation_Valid(string value)
        => Assert.That(InputExpression.Parser.Parse(value).IsImplicitFoldAggregation, Is.True);
}
