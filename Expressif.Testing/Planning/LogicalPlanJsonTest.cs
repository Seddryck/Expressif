using System.Text.Json;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class LogicalPlanJsonTest
{
    [Test]
    public void Serialize_SameNormalizedPlan_IsDeterministicAcrossAliasAndNamedOrder()
    {
        var canonical = Plan("add(value := 5, times := 2)");
        var alias = Plan("numeric-to-add(times := 2, value := 5)");

        Assert.That(LogicalPlanJson.Serialize(alias, indented: false),
            Is.EqualTo(LogicalPlanJson.Serialize(canonical, indented: false)));
    }

    [Test]
    public void Deserialize_SerializedPlan_RoundTripsCanonicalJson()
    {
        var json = LogicalPlanJson.Serialize(Plan("{1, 2} | map(.age) | add(5)"), indented: false);

        var roundTrip = LogicalPlanJson.Serialize(LogicalPlanJson.Deserialize(json), indented: false);

        Assert.That(roundTrip, Is.EqualTo(json));
    }

    [Test]
    public void Serialize_SemanticTemporalAndNullLiterals_RoundTripWithoutRuntimeMetadata()
    {
        LogicalValue[] values =
        [
            new LogicalLiteral("date", new DateOnly(2026, 9, 13)),
            new LogicalLiteral("datetime", new DateTime(2026, 9, 13, 12, 30, 0, DateTimeKind.Utc)),
            new LogicalLiteral("time", new TimeOnly(12, 30, 1, 250)),
            new LogicalLiteral("duration", TimeSpan.FromMinutes(90)),
            new LogicalLiteral("null", null),
        ];
        var plan = new LogicalPlan(new LogicalPipeline(values));

        var json = LogicalPlanJson.Serialize(plan, indented: false);
        var roundTrip = LogicalPlanJson.Serialize(LogicalPlanJson.Deserialize(json), indented: false);

        Assert.Multiple(() =>
        {
            Assert.That(roundTrip, Is.EqualTo(json));
            Assert.That(json, Does.Not.Contain("System."));
            Assert.That(json, Does.Not.Contain("Assembly"));
        });
    }

    [Test]
    public void Deserialize_UnsupportedVersion_ReportsSupportedVersion()
    {
        var json = LogicalPlanJson.Serialize(Plan("trim"), indented: false)
            .Replace("\"version\":1", "\"version\":2", StringComparison.Ordinal);

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Deserialize(json));

        Assert.That(exception!.Message, Is.EqualTo(
            "Unsupported logical-plan version '2'. This reader supports version '1'."));
    }

    [Test]
    public void ReadSchema_ExposesVersionedPortableSchema()
    {
        using var schema = JsonDocument.Parse(LogicalPlanJson.ReadSchema());

        Assert.Multiple(() =>
        {
            Assert.That(schema.RootElement.GetProperty("$id").GetString(), Does.EndWith("logical-plan-v1.schema.json"));
            Assert.That(schema.RootElement.GetProperty("properties").GetProperty("format").GetProperty("const").GetString(),
                Is.EqualTo(LogicalPlanJson.FormatName));
            Assert.That(schema.RootElement.GetProperty("properties").GetProperty("version").GetProperty("const").GetInt32(),
                Is.EqualTo(LogicalPlanJson.FormatVersion));
            Assert.That(schema.RootElement.GetProperty("$defs").GetProperty("value").GetProperty("oneOf").GetArrayLength(),
                Is.EqualTo(3));
        });
    }

    [Test]
    public void Serialize_PlanCarriesEvaluationScopeAndOmissionMetadata()
    {
        using var document = JsonDocument.Parse(LogicalPlanJson.Serialize(Plan("{1, 2} | map(.age) | add(5)")));
        var calls = document.RootElement.GetProperty("plan").GetProperty("items");
        var mapArgument = calls[1].GetProperty("arguments")[0];
        var omitted = calls[2].GetProperty("arguments")[1];

        Assert.Multiple(() =>
        {
            Assert.That(mapArgument.GetProperty("parameter").GetProperty("evaluation").GetProperty("context").GetString(),
                Is.EqualTo("traversal"));
            Assert.That(mapArgument.GetProperty("value").GetProperty("items")[0].GetProperty("operator").GetProperty("name").GetString(),
                Is.EqualTo("field"));
            Assert.That(omitted.GetProperty("omission").GetProperty("mode").GetString(), Is.EqualTo("constant"));
        });
    }

    private static LogicalPlan Plan(string source)
        => LogicalPlanner.Plan(ExpressionParser.Parse(source));
}
