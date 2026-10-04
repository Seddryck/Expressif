using Expressif.Bindings;
using System.Reflection;

namespace Expressif.Testing.Bindings;

public class ExpressionBinderTest
{
    [Test]
    public void ExpressionContainers_PublicSurfaceIsSealedAndMinimal()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(ExpressifBinder).IsNotPublic, Is.True);
            Assert.That(typeof(InputBoundExpression).IsNotPublic, Is.True);
            Assert.That(typeof(OpenExpression).IsSealed, Is.True);
            Assert.That(typeof(ClosedExpression).IsSealed, Is.True);
            Assert.That(typeof(ClosedExpression).GetProperty(
                "IsImplicitFoldAggregation", BindingFlags.Instance | BindingFlags.Public), Is.Null);
            Assert.That(typeof(ClosedExpression).GetMethod(
                "GetImplicitFoldAccumulator", BindingFlags.Instance | BindingFlags.Public), Is.Null);
        });
    }

    [Test]
    public void Bind_EnclosingRootReferenceAsArgument_CreatesEnclosingPropertyParameter()
    {
        var syntax = ExpressionParser.Parse("greater-than(^^.threshold)");
        var expression = ((OpenRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.That(expression.Members.Single().Parameters.Single(),
            Is.EqualTo(new EnclosingObjectPropertyParameter("threshold")));
    }

    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Bind_OpenFunction_ReturnsBoundFunction()
    {
        var syntax = SyntaxFactory.Open(null, SyntaxFactory.Function("upper"));
        var expression = ((OpenRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.That(expression.Members.Single().Name, Is.EqualTo("upper"));
    }

    [Test]
    public void Bind_OpenExpression_PreservesMembers()
    {
        var syntax = SyntaxFactory.Open(
            SyntaxFactory.Function("text-to-func", SyntaxFactory.Argument(SyntaxFactory.Text("foo"))),
            SyntaxFactory.Function("numeric-to-func", SyntaxFactory.Argument(SyntaxFactory.Variable("bar"))),
            SyntaxFactory.Function("boolean-to-func"));

        var expression = ((OpenRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.That(expression.Members.Select(member => member.Name),
            Is.EqualTo(new[] { "text-to-func", "numeric-to-func", "boolean-to-func" }));
    }

    [Test]
    public void Bind_ClosedExpression_PreservesSourceAndPipeline()
    {
        var syntax = SyntaxFactory.Closed(
            SyntaxFactory.Variable("foo"),
            SyntaxFactory.Function("text-to-func", SyntaxFactory.Argument(SyntaxFactory.Text("foo"))),
            SyntaxFactory.Function("numeric-to-func", SyntaxFactory.Argument(SyntaxFactory.Variable("bar"))));

        var expression = ((ClosedRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Parameter, Is.EqualTo(new VariableParameter("foo")));
            Assert.That(expression.Members.Select(member => member.Name),
                Is.EqualTo(new[] { "text-to-func", "numeric-to-func" }));
        });
    }

    [Test]
    public void Bind_InputExpression_ImplicitFoldAggregation_Valid()
    {
        var syntax = SyntaxFactory.Closed(
            SyntaxFactory.Array(SyntaxFactory.Number(1), SyntaxFactory.Number(2), SyntaxFactory.Number(3)),
            SyntaxFactory.Function("sum"));

        var expression = ((ClosedRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.That(expression.Members.Single().Role, Is.EqualTo(BoundFunctionRole.ImplicitAccumulator));
    }

    [Test]
    public void Parse_ClosedExpression_SumDetectedAsImplicitFoldAccumulator()
    {
        var syntax = SyntaxFactory.Closed(
            SyntaxFactory.Array(SyntaxFactory.Number(1), SyntaxFactory.Number(2), SyntaxFactory.Number(3)),
            SyntaxFactory.Function("sum"));
        var expression = ((ClosedRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        var accumulator = expression.Members.Single();

        Assert.Multiple(() =>
        {
            Assert.That(accumulator.Role, Is.EqualTo(BoundFunctionRole.ImplicitAccumulator));
            Assert.That(accumulator.Name, Is.EqualTo("sum"));
        });
    }

    [Test]
    public void Bind_RootExpression_DistinguishesOpenAndClosedSyntax()
    {
        var binder = ExpressifBinderFactory.Create();
        var closed = SyntaxFactory.Closed(SyntaxFactory.Variable("foo"), SyntaxFactory.Function("lower"));
        var open = SyntaxFactory.Open(SyntaxFactory.Function("sum"), SyntaxFactory.Function("add"));

        Assert.Multiple(() =>
        {
            Assert.That(binder.Bind(closed), Is.TypeOf<ClosedRootExpression>());
            Assert.That(binder.Bind(open), Is.TypeOf<OpenRootExpression>());
        });
    }

    [Test]
    public void Parse_RootRecordAccessAsPipelineStage_BindsRootFieldShorthand()
    {
        var syntax = SyntaxFactory.Open(null, SyntaxFactory.RecordAccess("firstName", true));
        var expression = ((OpenRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.That(expression.Members.Single().Notation, Is.EqualTo(SourceNotation.RootField));
    }

    [Test]
    public void Parse_MapShorthand_LowersToMapFunction()
    {
        var syntax = SyntaxFactory.Closed(
            SyntaxFactory.Array(SyntaxFactory.Number(1), SyntaxFactory.Number(2), SyntaxFactory.Number(3)),
            SyntaxFactory.Map(
                SyntaxFactory.Function("absolute"),
                SyntaxFactory.Function("add", SyntaxFactory.Argument(SyntaxFactory.Number(5)))),
            SyntaxFactory.Function("reverse"));
        var expression = ((ClosedRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "reverse" }));
            Assert.That(expression.Members.First().Notation, Is.EqualTo(SourceNotation.MapShorthand));
            Assert.That(((OpenExpressionParameter)expression.Members.First().Parameters.Single()).Expression.Members.Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public void Parse_LeadingMapPipeline_ResumesParentPipelineAfterMappedExpression()
    {
        var syntax = SyntaxFactory.Open(
            SyntaxFactory.Map(
                SyntaxFactory.Function("absolute"),
                SyntaxFactory.Function("add", SyntaxFactory.Argument(SyntaxFactory.Number(1)))),
            SyntaxFactory.Function("sum"));
        var root = ExpressifBinderFactory.Create().Bind(syntax);

        Assert.That(root, Is.TypeOf<OpenRootExpression>());
        var expression = ((OpenRootExpression)root).Expression;
        var map = expression.Members.First();
        var mappedExpression = ((OpenExpressionParameter)map.Parameters.Single()).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "sum" }));
            Assert.That(map.Name, Is.EqualTo("map"));
            Assert.That(map.Notation, Is.EqualTo(SourceNotation.MapShorthand));
            Assert.That(mappedExpression.Members.Select(x => x.Name), Is.EqualTo(new[] { "absolute", "add" }));
        });
    }

    [Test]
    public void Parse_UnparenthesizedMapShorthand_ConsumesSingleFunction()
    {
        var syntax = SyntaxFactory.Closed(
            SyntaxFactory.Array(SyntaxFactory.Number(1), SyntaxFactory.Number(2), SyntaxFactory.Number(3)),
            SyntaxFactory.Map(SyntaxFactory.Function("absolute")),
            SyntaxFactory.Function("add", SyntaxFactory.Argument(SyntaxFactory.Number(1))),
            SyntaxFactory.Function("sum"));
        var expression = ((ClosedRootExpression)ExpressifBinderFactory.Create().Bind(syntax)).Expression;

        var map = expression.Members.First();
        var mappedExpression = ((OpenExpressionParameter)map.Parameters.Single()).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "map", "add", "sum" }));
            Assert.That(mappedExpression.Members.Select(x => x.Name), Is.EqualTo(new[] { "absolute" }));
        });
    }

    [Test]
    public void Parse_AdjacentOpenComposition_PreservesTupleProjections()
    {
        var composition = SyntaxFactory.Open(
            SyntaxFactory.TupleProjection(1),
            SyntaxFactory.Function("subtract", SyntaxFactory.Argument(SyntaxFactory.TupleProjection(0))),
            SyntaxFactory.Function("multiply", SyntaxFactory.Argument(SyntaxFactory.TupleProjection(1))));
        var syntax = SyntaxFactory.Open(
            SyntaxFactory.Function("adjacent", SyntaxFactory.Argument(SyntaxFactory.Parenthesized(composition))));
        var adjacent = ExpressifBinderFactory.Create().BindSingleFunction(syntax);
        var expression = ((OpenExpressionParameter)adjacent.Parameters.Single()).Expression;

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members.Select(x => x.Name),
                Is.EqualTo(new[] { "tuple-at", "subtract", "multiply" }));
            Assert.That(((LiteralParameter)expression.Members.First().Parameters.Single()).Value,
                Is.EqualTo("1"));
            Assert.That(expression.Members.Skip(1).Select(x => ((TupleProjectionParameter)x.Parameters.Single()).Index),
                Is.EqualTo(new[] { 0, 1 }));
        });
    }
}
