using System.Text.Json;
using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class LogicalPlanJsonTest
{
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
    public void Serialize_AllSupportedLiteralRuntimeTypes_WritesExpectedJsonValues()
    {
        LogicalValue[] values =
        [
            new LogicalLiteral("null", null),
            new LogicalLiteral("boolean", true),
            new LogicalLiteral("text", "value"),
            new LogicalLiteral("decimal", 12.5m),
            new LogicalLiteral("integer", 12),
            new LogicalLiteral("integer", 13L),
            new LogicalLiteral("date", new DateOnly(2026, 9, 13)),
            new LogicalLiteral("datetime", new DateTime(2026, 9, 13, 12, 30, 0, DateTimeKind.Utc)),
            new LogicalLiteral("time", new TimeOnly(12, 30, 1, 250)),
            new LogicalLiteral("duration", TimeSpan.FromMinutes(90)),
        ];

        using var document = JsonDocument.Parse(LogicalPlanJson.Serialize(
            new LogicalPlan(new LogicalPipeline(values)),
            indented: false));
        var serialized = document.RootElement.GetProperty("plan").GetProperty("items")
            .EnumerateArray().Select(item => item.GetProperty("value").ToString()).ToArray();

        Assert.That(serialized, Is.EqualTo(new[]
        {
            string.Empty, "True", "value", "12.5", "12", "13", "2026-09-13",
            "2026-09-13T12:30:00.0000000Z", "12:30:01.2500000", "01:30:00",
        }));
    }

    [Test]
    public void Serialize_UnsupportedLiteralRuntimeType_ThrowsFormatException()
    {
        var plan = new LogicalPlan(new LogicalPipeline([new LogicalLiteral("custom", new Version(1, 2))]));

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Serialize(plan));

        Assert.That(exception!.Message, Is.EqualTo("Literal type 'custom' contains unsupported value 'Version'."));
    }

    [Test]
    public void Deserialize_InvalidJson_WrapsJsonException()
    {
        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Deserialize("{"));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Message, Is.EqualTo("The logical-plan document is not valid JSON."));
            Assert.That(exception.InnerException, Is.InstanceOf<JsonException>());
        });
    }

    [TestCase("boolean", "\"not-a-boolean\"", typeof(InvalidOperationException))]
    [TestCase("date", "\"not-a-date\"", typeof(FormatException))]
    [TestCase("integer", "9223372036854775808", typeof(FormatException))]
    [TestCase("duration", "\"999999999999999999999.00:00:00\"", typeof(OverflowException))]
    public void Deserialize_InvalidLiteralValue_WrapsConversionException(
        string type,
        string value,
        Type expectedInnerException)
    {
        var json = "{\"format\":\"expressif.logical-plan\",\"version\":1,\"catalogCompatibility\":\"3.0\"," +
            $"\"plan\":{{\"kind\":\"pipeline\",\"items\":[{{\"kind\":\"literal\",\"type\":\"{type}\",\"value\":{value}}}]}}}}";

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Deserialize(json));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Message, Is.EqualTo("The logical-plan document contains an invalid value."));
            Assert.That(exception.InnerException, Is.TypeOf(expectedInnerException));
        });
    }

    [Test]
    public void Deserialize_UnsupportedVersion_ReportsSupportedVersion()
    {
        var json = LogicalPlanJson.Serialize(new LogicalPlan(new LogicalPipeline([])), indented: false)
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
            Assert.That(schema.RootElement.GetProperty("$defs").GetProperty("semantics")
                    .GetProperty("properties").GetProperty("cardinality").GetProperty("enum").GetArrayLength(),
                Is.EqualTo(6));
            Assert.That(schema.RootElement.GetProperty("$defs").GetProperty("traversal")
                    .GetProperty("properties").TryGetProperty("summary", out _),
                Is.False);
            Assert.That(schema.RootElement.GetProperty("$defs").GetProperty("evaluation")
                    .GetProperty("properties").TryGetProperty("summary", out _),
                Is.False);
        });
    }

    [Test]
    public void Deserialize_UnsupportedStructuralSemantics_ThrowsFormatException()
    {
        var plan = new LogicalPlan(new LogicalPipeline([
            new LogicalCall(new PlannerFunctionDescriptor("map", "array", "array",
                Semantics: new PlannerSemanticsDescriptor("preserved", "per-element", "preserved")), []),
        ]));
        var json = LogicalPlanJson.Serialize(plan, indented: false)
            .Replace("\"cardinality\":\"preserved\"", "\"cardinality\":\"invalid\"", StringComparison.Ordinal);

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Deserialize(json));

        Assert.That(exception!.Message, Is.EqualTo("Unsupported semantics cardinality 'invalid'."));
    }
}
