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
        => Assert.That(OpenExpression.Parser.Parse(value).Members.Count, Is.EqualTo(count));

    [Test]
    [TestCase("@foo | text-to-func(foo, @bar)", 1)]
    [TestCase("@foo | text-to-func(foo) | numeric-to-func(foo, @bar)", 2)]
    [TestCase("foo", 0)]
    public void Parse_ParametrizedExpression_Valid(string value, int count)
        => Assert.That(Expressif.Parsers.ClosedExpression.Parser.Parse(value).Members.Count, Is.EqualTo(count));

    [Test]
    [TestCase("{1,2,3} | sum")]
    [TestCase("@foo | count")]
    [TestCase("[foo] | min")]
    [TestCase("#1 | last")]
    [TestCase("{true,true} | every")]
    [TestCase("{false,true} | any")]
    public void Parse_InputExpression_ImplicitFoldAggregation_Valid(string value)
        => Assert.That(Expressif.Parsers.ClosedExpression.Parser.Parse(value).IsImplicitFoldAggregation, Is.True);

    [Test]
    public void Parse_ClosedExpression_SumDetectedAsImplicitFoldAccumulator()
    {
        var expression = Expressif.Parsers.ClosedExpression.Parser.Parse("{1,2,3} | sum");

        Assert.That(expression.IsImplicitFoldAggregation, Is.True);
        Assert.That(expression.GetImplicitFoldAccumulator(), Is.Not.Null);
        Assert.That(expression.GetImplicitFoldAccumulator()!.Name, Is.EqualTo("sum"));
    }

    [Test]
    [TestCase("@foo | lower", typeof(ClosedRootExpression))]
    [TestCase("{1,2,3} | sum", typeof(ClosedRootExpression))]
    [TestCase("@arr | count", typeof(ClosedRootExpression))]
    [TestCase("{10,20,30} | lag", typeof(ClosedRootExpression))]
    [TestCase("{10,20,30} | lead", typeof(ClosedRootExpression))]
    [TestCase("{1,2,3} | scan(sum)", typeof(ClosedRootExpression))]
    [TestCase("sum | add(3)", typeof(OpenRootExpression))]
    [TestCase("lower(foo) | trim", typeof(OpenRootExpression))]
    [TestCase("{1,2,3} | broadcast(sum)", typeof(ClosedRootExpression))]
    [TestCase("{1,2,3} | map(multiply(2))", typeof(ClosedRootExpression))]
    [TestCase("{`alice`,`bob`} | map(upper | first-chars(2))", typeof(ClosedRootExpression))]
    [TestCase("{1,2,3,4} | filter(greater-than(2))", typeof(ClosedRootExpression))]
    public void Parse_RootExpression_ClosedFirst(string value, Type expectedType)
        => Assert.That(RootExpression.Parser.Parse(value), Is.TypeOf(expectedType));

    [Test]
    public void Parse_MapShorthand_LowersToMapFunction()
    {
        var expression = Expressif.Parsers.ClosedExpression.Parser.End()
            .Parse("{1,2,3} |> (absolute | add(5)) | reverse");

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "reverse" }));
            Assert.That(expression.Members.First().Syntax, Is.EqualTo(FunctionSyntax.MapShorthand));
            Assert.That(((OpenExpressionParameter)expression.Members.First().Parameters.Single()).Expression.Members.Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public void Parse_LeadingMapPipeline_LowersEntirePipelineToMapFunction()
    {
        var root = RootExpression.Parser.Parse("|> add(1) | sum");

        Assert.That(root, Is.TypeOf<OpenRootExpression>());
        var expression = ((OpenRootExpression)root).Expression;
        var map = expression.Members.Single();
        var mappedExpression = ((OpenExpressionParameter)map.Parameters.Single()).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(map.Name, Is.EqualTo("map"));
            Assert.That(map.Syntax, Is.EqualTo(FunctionSyntax.MapShorthand));
            Assert.That(mappedExpression.Members.Select(x => x.Name), Is.EqualTo(new[] { "add", "sum" }));
        });
    }

    [TestCase("{1,2,3} |> absolute")]
    [TestCase("{1,2,3} |> ()")]
    [TestCase("{1,2,3} |> (absolute")]
    public void Parse_MapShorthandWithoutParenthesizedExpression_Invalid(string value)
        => Assert.That(() => Expressif.Parsers.ClosedExpression.Parser.End().Parse(value), Throws.TypeOf<ParseException>());
}
