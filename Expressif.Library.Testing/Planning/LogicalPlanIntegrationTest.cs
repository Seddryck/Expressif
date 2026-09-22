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

    private static LogicalPlan Plan(string source)
        => LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source));
}
