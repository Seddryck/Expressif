using Expressif.Bindings;
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
        => Assert.That(BindingTestAdapter.Open(value).Members.Count, Is.EqualTo(count));

    [Test]
    [TestCase("@foo | text-to-func(foo, @bar)", 1)]
    [TestCase("@foo | text-to-func(foo) | numeric-to-func(foo, @bar)", 2)]
    [TestCase("foo", 0)]
    public void Parse_ParametrizedExpression_Valid(string value, int count)
        => Assert.That(BindingTestAdapter.Closed(value).Members.Count, Is.EqualTo(count));

    [Test]
    [TestCase("{1,2,3} | sum")]
    [TestCase("@foo | count")]
    [TestCase("[foo] | min")]
    [TestCase("#1 | last")]
    [TestCase("{true,true} | every")]
    [TestCase("{false,true} | any")]
    public void Parse_InputExpression_ImplicitFoldAggregation_Valid(string value)
        => Assert.That(BindingTestAdapter.Closed(value).IsImplicitFoldAggregation, Is.True);

    [Test]
    public void Parse_ClosedExpression_SumDetectedAsImplicitFoldAccumulator()
    {
        var expression = BindingTestAdapter.Closed("{1,2,3} | sum");

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
        => Assert.That(BindingTestAdapter.Root(value), Is.TypeOf(expectedType));

    [Test]
    public void Parse_MapShorthand_LowersToMapFunction()
    {
        var expression = BindingTestAdapter.Closed("{1,2,3} |> (absolute | add(5)) | reverse");

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "reverse" }));
            Assert.That(expression.Members.First().Syntax, Is.EqualTo(FunctionSyntax.MapShorthand));
            Assert.That(((OpenExpressionParameter)expression.Members.First().Parameters.Single()).Expression.Members.Count(), Is.EqualTo(2));
        });
    }

    [Test]
    [TestCase("|> add(1) | sum", new[] { "add" })]
    [TestCase("|> (add(1)) | sum", new[] { "add" })]
    [TestCase("|> (absolute | add(1)) | sum", new[] { "absolute", "add" })]
    public void Parse_LeadingMapPipeline_ResumesParentPipelineAfterMappedExpression(
        string value, string[] expectedMappedFunctions)
    {
        var root = BindingTestAdapter.Root(value);

        Assert.That(root, Is.TypeOf<OpenRootExpression>());
        var expression = ((OpenRootExpression)root).Expression;
        var map = expression.Members.First();
        var mappedExpression = ((OpenExpressionParameter)map.Parameters.Single()).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "sum" }));
            Assert.That(map.Name, Is.EqualTo("map"));
            Assert.That(map.Syntax, Is.EqualTo(FunctionSyntax.MapShorthand));
            Assert.That(mappedExpression.Members.Select(x => x.Name), Is.EqualTo(expectedMappedFunctions));
        });
    }

    [TestCase("{1,2,3} |> ()")]
    [TestCase("{1,2,3} |> (absolute")]
    public void Parse_MapShorthandWithInvalidExpression_Invalid(string value)
        => Assert.That(() => BindingTestAdapter.Closed(value), Throws.TypeOf<ExpressifSyntaxException>());

    [Test]
    public void Parse_UnparenthesizedMapShorthand_ConsumesSingleFunction()
    {
        var expression = BindingTestAdapter.Closed("{1,2,3} |> absolute | add(1) | sum");

        var map = expression.Members.First();
        var mappedExpression = ((OpenExpressionParameter)map.Parameters.Single()).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "add", "sum" }));
            Assert.That(mappedExpression.Members.Select(x => x.Name), Is.EqualTo(new[] { "absolute" }));
        });
    }
}
