using Expressif.Bindings;
using Expressif.Library.Catalog;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Syntax;
using Expressif.Values.Formatting;

namespace Expressif.Testing.Planning;

public class LogicalPlanBinderTest
{
    [TestCase("upper", "alice", "ALICE")]
    [TestCase("1 | add(2)", null, "3")]
    [TestCase("{1, 2, 3} | sum", null, "6")]
    [TestCase("{1, 2, 3} | map(add(1))", null, "{2, 3, 4}")]
    [TestCase("apply(@_ | input :> @input | add(2))", 10, "12")]
    [TestCase("T(10, 20) | apply($0 | add($1))", null, "30")]
    [TestCase("{name := \"Alice\"} | .name | upper", null, "ALICE")]
    [TestCase("sort-by($0 -> :integer)", new object[] { null! }, "{null}")]
    public void Bind_Plan_EvaluatesWithoutSyntaxBinding(string source, object? input, string expected)
    {
        var expression = new LogicalPlanBinder().Bind(Plan(source));

        var actual = ValueFormatter.Format(expression.Evaluate(input));

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Bind_DeserializedPlan_EvaluatesWithoutOriginalSyntax()
    {
        var serialized = LogicalPlanJson.Serialize(Plan("{1, 2, 3} | map(add(1))"), indented: false);
        var deserialized = LogicalPlanJson.Deserialize(serialized);

        var actual = new LogicalPlanBinder().Bind(deserialized).Evaluate(null);

        Assert.That(ValueFormatter.Format(actual), Is.EqualTo("{2, 3, 4}"));
    }

    [Test]
    public void BindClosed_ArrayValuePlan_Evaluates()
    {
        var actual = new LogicalPlanBinder().BindClosed(Plan("{1, 2, 3} | sum")).Evaluate(null);

        Assert.That(actual, Is.EqualTo(6));
    }

    [Test]
    public void BindClosed_OpenArrayCallAndClosedArrayLiteral_AreIndistinguishable()
    {
        var open = Plan("array(1)");
        var closed = Plan("{1}");

        Assert.Multiple(() =>
        {
            Assert.That(LogicalPlanJson.Serialize(open, indented: false),
                Is.EqualTo(LogicalPlanJson.Serialize(closed, indented: false)));
            Assert.That(
                ValueFormatter.Format(new LogicalPlanBinder().BindClosed(open).Evaluate(null)),
                Is.EqualTo("{1}"));
        });
    }

    [Test]
    public void Bind_TypedPipeline_MatchesLegacyRuntimeBinding()
    {
        const string source = "trim | multiply(1.21) | round(2) | prepend(\"€\")";
        var plan = Plan(source);

        var actual = new LogicalPlanBinder().Bind(plan).Evaluate(" 10 ");
        var expected = ExpressionBinder.Bind(ExpressionParser.Parse(source)).Evaluate(" 10 ");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("is-within-interval(I]0, 10[) ?> add(2)", 5)]
    [TestCase("absolute <? is-within-interval(I[0, 10[)", -5)]
    [TestCase("switch(is-positive => 1, greater-than(10) => 2, _ => 3)", 20)]
    [TestCase("try(absolute => greater-than(10), neutral => is-negative)", -5)]
    [TestCase("let(value := 10) | add(@value)", 5)]
    [TestCase("map-with(~subtract, {10, 11})", 5)]
    [TestCase("{3, 1, 2} | sort-by(@_ -> :integer)", null)]
    [TestCase("#less", null)]
    [TestCase("#all", null)]
    public void Bind_RepresentativePlan_MatchesLegacyBinding(string source, object? input)
    {
        var actual = new LogicalPlanBinder().Bind(Plan(source)).Evaluate(input);
        var expected = ExpressionBinder.Bind(ExpressionParser.Parse(source)).Evaluate(input);

        Assert.That(ValueFormatter.Format(actual), Is.EqualTo(ValueFormatter.Format(expected)));
    }

    [Test]
    public void Plan_WithDefinition_UsesUpdatedCatalogShape()
        => Assert.That(() => Plan("with(v := add(1), multiply(@v))"), Throws.Nothing);

    [Test]
    public void Audit_DocumentedExpressions_CanBePlannedAndBound()
    {
        var expectedPlanningFailures = new[] { "chunk-while" };
        var expectedBindingFailures = new[]
        {
            "put",
            "put-absent",
            "put-present",
            "satisfies-at-least",
            "satisfies-at-most",
            "satisfies-exactly",
            "transform-as",
            "with",
        };
        var planningFailures = new List<string>();
        var planningFailureFunctions = new List<string>();
        var bindingFailures = new List<string>();
        var bindingFailureFunctions = new List<string>();
        foreach (var function in FunctionCatalog.Default.Functions)
        {
            foreach (var example in function.Examples ?? [])
            {
                var separator = example.IndexOf('→');
                var source = separator < 0 ? example : example[..separator].TrimEnd();
                LogicalPlan plan;
                try
                {
                    plan = Plan(source);
                }
                catch (Exception exception)
                {
                    planningFailureFunctions.Add(function.Name);
                    planningFailures.Add($"{function.Name}: {source}: {exception.GetType().Name}: {exception.Message}");
                    continue;
                }

                try
                {
                    _ = new LogicalPlanBinder().Bind(plan);
                }
                catch (Exception exception)
                {
                    bindingFailureFunctions.Add(function.Name);
                    bindingFailures.Add($"{function.Name}: {source}: {exception.GetType().Name}: {exception.Message}");
                }
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(
                planningFailureFunctions.Distinct().Order().ToArray(),
                Is.EqualTo(expectedPlanningFailures.Order().ToArray()),
                string.Join(Environment.NewLine, planningFailures.Distinct()));
            Assert.That(
                bindingFailureFunctions.Distinct().Order().ToArray(),
                Is.EqualTo(expectedBindingFailures.Order().ToArray()),
                string.Join(Environment.NewLine, bindingFailures.Distinct()));
        });
    }

    private static LogicalPlan Plan(string source)
        => LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source));
}
