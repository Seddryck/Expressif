using System.Text.Json;
using Expressif.Library.Composition;
using Expressif.Library.Catalog;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class LogicalPlanIntegrationTest
{
    [Test]
    public void BuiltInAssembly_DoesNotDuplicatePlanningTypesOrSchema()
    {
        var library = typeof(FunctionCatalog).Assembly;

        Assert.Multiple(() =>
        {
            Assert.That(library.GetType(typeof(LogicalPlanner).FullName!), Is.Null);
            Assert.That(library.GetManifestResourceNames(), Does.Not.Contain(LogicalPlanJson.SchemaResourceName));
        });
    }

    [Test]
    public void Serialize_SameNormalizedPlan_IsDeterministicAcrossAliasAndNamedOrder()
    {
        var canonical = Plan("add(value := 5, times := 2)");
        var alias = Plan("numeric-to-add(times := 2, value := 5)");

        Assert.That(LogicalPlanJson.Serialize(alias, indented: false),
            Is.EqualTo(LogicalPlanJson.Serialize(canonical, indented: false)));
    }

    [Test]
    public void Deserialize_SerializedBuiltInPlan_RoundTripsCanonicalJson()
    {
        var json = LogicalPlanJson.Serialize(Plan("{1, 2} | map(.age) | add(5)"), indented: false);

        var roundTrip = LogicalPlanJson.Serialize(LogicalPlanJson.Deserialize(json), indented: false);

        Assert.That(roundTrip, Is.EqualTo(json));
    }

    [Test]
    public void Serialize_BuiltInPlanCarriesSemanticsEvaluationScopeAndOmissionMetadata()
    {
        var json = LogicalPlanJson.Serialize(Plan("{1, 2} | map(.age) | add(5)"));
        using var document = JsonDocument.Parse(json);
        var calls = document.RootElement.GetProperty("plan").GetProperty("items");
        var mapOperator = calls[1].GetProperty("operator");
        var mapArgument = calls[1].GetProperty("arguments")[0];
        var omitted = calls[2].GetProperty("arguments")[1];

        Assert.Multiple(() =>
        {
            Assert.That(mapOperator.GetProperty("semantics").GetProperty("cardinality").GetString(),
                Is.EqualTo("preserved"));
            Assert.That(mapOperator.GetProperty("semantics").GetProperty("dependency").GetString(),
                Is.EqualTo("per-element"));
            Assert.That(mapOperator.GetProperty("semantics").GetProperty("ordering").GetString(),
                Is.EqualTo("preserved"));
            Assert.That(mapArgument.GetProperty("parameter").GetProperty("evaluation").GetProperty("context").GetString(),
                Is.EqualTo("traversal"));
            Assert.That(mapArgument.GetProperty("value").GetProperty("items")[0].GetProperty("operator").GetProperty("name").GetString(),
                Is.EqualTo("field"));
            Assert.That(omitted.GetProperty("omission").GetProperty("mode").GetString(), Is.EqualTo("constant"));
            Assert.That(json, Does.Not.Contain("\"summary\""));
        });
    }

    [TestCase("apply(@_ | input :> @input)")]
    [TestCase("apply(@_ | :> .first | upper)")]
    [TestCase("apply((left, right) :> @left)")]
    [TestCase("apply(@_ | outer :> apply(@_ | inner :> @outer))")]
    public void Deserialize_SerializedInputBinding_RoundTripsCanonicalJson(string source)
    {
        var json = LogicalPlanJson.Serialize(Plan(source), indented: false);

        var roundTrip = LogicalPlanJson.Serialize(LogicalPlanJson.Deserialize(json), indented: false);

        Assert.Multiple(() =>
        {
            Assert.That(roundTrip, Is.EqualTo(json));
            Assert.That(json, Does.Contain("\"name\":\"input-binding\""));
            Assert.That(json, Does.Contain("\"name\":\"body\""));
        });
    }

    [Test]
    public void Deserialize_InputBindingWithInvalidArguments_ThrowsFormatException()
    {
        var json = LogicalPlanJson.Serialize(Plan("apply(@_ | input :> @input)"), indented: false)
            .Replace("\"name\":\"positional\"", "\"name\":\"mode\"", StringComparison.Ordinal);

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Deserialize(json));

        Assert.That(exception!.Message, Is.EqualTo(
            "An input-binding call must contain names, positional, and body arguments in that order."));
    }

    [TestCase("#all", "all", "#all")]
    [TestCase("#less", "ordering", "#less")]
    [TestCase("#equal", "ordering", "#equal")]
    [TestCase("#greater", "ordering", "#greater")]
    public void Serialize_SpecialScalarLiteral_RoundTripsCanonicalJson(
        string source,
        string expectedType,
        string expectedValue)
    {
        var json = LogicalPlanJson.Serialize(Plan(source), indented: false);
        var roundTrip = LogicalPlanJson.Deserialize(json);
        var literal = (LogicalLiteral)roundTrip.Pipeline.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(literal.Type, Is.EqualTo(expectedType));
            Assert.That(literal.Value, Is.EqualTo(expectedValue));
            Assert.That(LogicalPlanJson.Serialize(roundTrip, indented: false), Is.EqualTo(json));
        });
    }

    private static LogicalPlan Plan(string source)
        => LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source));
}
