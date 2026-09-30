using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Syntax;
using Expressif.Values;

namespace Expressif.Testing.Bindings;

public sealed class LogicalPlanBinderTest
{
    [Test]
    public void Bind_Syntax_UsesLogicalPlanPipeline()
    {
        var expression = ExpressionBinder.Bind(ExpressionParser.Parse("1 | add(2)"));

        Assert.That(expression.Evaluate(null), Is.EqualTo(3m));
    }

    [Test]
    public void BindClosed_Syntax_UsesLogicalPlanPipeline()
    {
        var expression = ExpressionBinder.BindClosed(ExpressionParser.Parse("1 | add(2)"));

        Assert.That(expression.Evaluate(null), Is.EqualTo(3m));
    }

    [TestCase("add(2)", 3, 5)]
    [TestCase("1 | add(2)", null, 3)]
    [TestCase("\"answer\" | upper", null, "ANSWER")]
    [TestCase("\"42\" | coerce(:integer)", null, 42)]
    [TestCase("apply(@_ | value :> add(@value))", 3, 6)]
    [TestCase("with(first := 1, second := 2, .first | add(.second))", null, 3)]
    [TestCase("is-within-interval(I]0, 10[) ?> add(2)", 5, 7)]
    [TestCase("absolute <? is-within-interval(I[0, 10[)", -5, 5)]
    [TestCase("switch(is-within-interval(I]0, 10[) => add(2), _ => 0)", 5, 7)]
    public void Bind_RoundTrippedPlan_MatchesSyntaxExecution(
        string source,
        object? input,
        object? expected)
    {
        var expression = ExpressionBinder.Bind(RoundTrip(source));

        Assert.That(expression.Evaluate(Normalize(input)), Is.EqualTo(Normalize(expected)));
    }

    [Test]
    public void Bind_RoundTrippedCollectionPredicateAndAccumulator_MatchesSyntaxExecution()
    {
        var predicate = ExpressionBinder.Bind(RoundTrip("{1, 2, 3} | filter(greater-than(1))"));
        var accumulator = ExpressionBinder.Bind(RoundTrip("{1, 2, 3} | sum"));
        var crossKindAccumulator = ExpressionBinder.Bind(RoundTrip("{1, 2, 3} | fold(first)"));

        Assert.Multiple(() =>
        {
            Assert.That(predicate.Evaluate(null), Is.EqualTo(new object[] { 2m, 3m }));
            Assert.That(accumulator.Evaluate(null), Is.EqualTo(6m));
            Assert.That(crossKindAccumulator.Evaluate(null), Is.EqualTo(1m));
        });
    }

    [Test]
    public void Bind_RoundTrippedNamedEntryFunctions_MatchSyntaxExecution()
    {
        var put = ExpressionBinder.Bind(RoundTrip("record(value := 1) | put(other := 2)"));
        var transform = ExpressionBinder.Bind(RoundTrip(
            "record(first-name := \"Ada\", last-name := \"Lovelace\") "
            + "| transform-as(upper, first := .first-name, last := .last-name)"));

        Assert.Multiple(() =>
        {
            Assert.That(((RecordValue)put.Evaluate(null)!).Keys, Is.EqualTo(new[] { "value", "other" }));
            var transformed = (RecordValue)transform.Evaluate(null)!;
            Assert.That(transformed["first"], Is.EqualTo("ADA"));
            Assert.That(transformed["last"], Is.EqualTo("LOVELACE"));
        });
    }

    [Test]
    public void Bind_RoundTrippedNestedContextReferences_PreserveDepth()
    {
        var item = new RecordValue();
        item.Set("amount", 42m);
        var input = new RecordValue();
        input.Set("country", "BE");
        input.Set("items", new object?[] { item });
        var expression = ExpressionBinder.Bind(RoundTrip(
            ".items | map(pair(^.amount, ^^.country))"));

        Assert.That(expression.Evaluate(input),
            Is.EqualTo(new object?[] { new PairValue(42m, "BE") }));
    }

    [Test]
    public void Bind_RoundTrippedSortCriterion_PreservesTupleSelector()
    {
        var input = new object?[]
        {
            new TupleValue(2m, "second"),
            new TupleValue(1m, "first"),
        };
        var expression = ExpressionBinder.Bind(RoundTrip("sort-by($0 -> :decimal)"));

        Assert.That(expression.Evaluate(input), Is.EqualTo(new object?[]
        {
            new TupleValue(1m, "first"),
            new TupleValue(2m, "second"),
        }));
    }

    [Test]
    public void BindClosed_OpenPlan_ThrowsPlanBindingException()
    {
        var exception = Assert.Throws<LogicalPlanBindingException>(
            () => ExpressionBinder.BindClosed(RoundTrip("upper")));

        Assert.That(exception!.Message, Does.Contain("caller-supplied pipeline input"));
    }

    [Test]
    public void BindClosed_StructuralSourceUsingIncomingValue_ThrowsPlanBindingException()
    {
        var exception = Assert.Throws<LogicalPlanBindingException>(
            () => ExpressionBinder.BindClosed(RoundTrip("array(@_)")));

        Assert.That(exception!.Message, Does.Contain("caller-supplied pipeline input"));
    }

    [Test]
    public void BindClosed_ClosedRoundTrippedPlan_ExecutesWithoutCallerInput()
    {
        var expression = ExpressionBinder.BindClosed(RoundTrip("1 | add(2)"));

        Assert.That(expression.Evaluate(null), Is.EqualTo(3m));
    }

    [Test]
    public void Bind_ExtensionPlan_UsesExplicitExtensionTypeSource()
    {
        var plan = new LogicalPlan(new LogicalPipeline([
            new LogicalCall(
                new PlannerFunctionDescriptor("triple", "decimal", "decimal", Kind: "extension"),
                [])
        ]));
        IExpressionBinder binder = new ExpressionBinder(new FixedTypeSource(typeof(Triple)));

        Assert.That(binder.Bind(plan).Evaluate(4m), Is.EqualTo(12m));
    }

    [Test]
    public void Bind_UnresolvedExtension_ThrowsPlanBindingException()
    {
        var plan = new LogicalPlan(new LogicalPipeline([
            new LogicalCall(
                new PlannerFunctionDescriptor("missing-extension", "any", "any", Kind: "extension"),
                [])
        ]));

        var exception = Assert.Throws<LogicalPlanBindingException>(() => ExpressionBinder.Bind(plan));

        Assert.That(exception!.Message, Does.Contain("missing-extension").And.Contain("not registered"));
    }

    private static LogicalPlan RoundTrip(string source)
    {
        var plan = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source));
        return LogicalPlanJson.Deserialize(LogicalPlanJson.Serialize(plan));
    }

    private static object? Normalize(object? value)
        => value is int number ? (decimal)number : value;

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    [Function(prefix: "", aliases: [])]
    private sealed class Triple : Function<decimal, decimal>
    {
        public override decimal Evaluate(decimal value) => value * 3;
    }
}
