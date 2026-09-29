using System.Text.Json;
using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class SchemaAnalysisJsonTest
{
    [Test]
    public void Serialize_WritesDeterministicExpandedSchemaDocument()
    {
        var analysis = new SchemaAnalysis(
            new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
            {
                ["b"] = new(new ScalarLogicalSchema("integer")),
                ["a"] = new(new ScalarLogicalSchema("text"), Optional: true),
            }),
            new ScalarLogicalSchema("integer", true),
            SchemaAnalysisCompleteness.Known,
            [
                new SchemaAnalysisDiagnostic("schema.second", "plan.items[1]", "Second diagnostic."),
                new SchemaAnalysisDiagnostic("schema.first", "plan.items[0]", "First diagnostic."),
            ],
            [
                new SchemaAnalysisNode(
                    "plan.items[1]",
                    "call",
                    "length",
                    new ScalarLogicalSchema("text", true),
                    new ScalarLogicalSchema("integer", true)),
                new SchemaAnalysisNode(
                    "plan.items[0]",
                    "literal",
                    null,
                    new NoInputLogicalSchema(),
                    new ScalarLogicalSchema("text")),
            ]);

        var json = SchemaAnalysisJson.Serialize(analysis, indented: false);

        Assert.That(json, Is.EqualTo(
            "{\"format\":\"expressif.schema-analysis\",\"version\":1,"
            + "\"input\":{\"type\":\"record\",\"nullable\":false,\"additionalFields\":true,\"fields\":{"
            + "\"a\":{\"optional\":true,\"schema\":{\"type\":\"text\",\"nullable\":false}},"
            + "\"b\":{\"optional\":false,\"schema\":{\"type\":\"integer\",\"nullable\":false}}}},"
            + "\"output\":{\"type\":\"integer\",\"nullable\":true},\"completeness\":\"known\",\"nodes\":["
            + "{\"path\":\"plan.items[0]\",\"kind\":\"literal\",\"input\":{\"type\":\"none\"},"
            + "\"output\":{\"type\":\"text\",\"nullable\":false}},"
            + "{\"path\":\"plan.items[1]\",\"kind\":\"call\",\"operator\":\"length\","
            + "\"input\":{\"type\":\"text\",\"nullable\":true},"
            + "\"output\":{\"type\":\"integer\",\"nullable\":true}}],\"diagnostics\":["
            + "{\"code\":\"schema.first\",\"path\":\"plan.items[0]\",\"message\":\"First diagnostic.\"},"
            + "{\"code\":\"schema.second\",\"path\":\"plan.items[1]\",\"message\":\"Second diagnostic.\"}]}"));
    }

    [Test]
    public void Serialize_EquivalentAnalysis_IsByteForByteDeterministic()
    {
        var analysis = LogicalSchemaAnalyzer.Analyze(
            LogicalPlanner.Plan(ExpressionParser.Parse(".customer | upper | length")));

        var first = SchemaAnalysisJson.Serialize(analysis);
        var second = SchemaAnalysisJson.Serialize(analysis);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Serialize_GroupingAndUnion_UsesExpandedShapes()
    {
        var analysis = new SchemaAnalysis(
            new NoInputLogicalSchema(),
            new GroupingLogicalSchema(
                new UnionLogicalSchema([
                    new ScalarLogicalSchema("text"),
                    new ScalarLogicalSchema("integer"),
                ]),
                new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>())),
            SchemaAnalysisCompleteness.Known,
            [],
            []);

        using var document = JsonDocument.Parse(SchemaAnalysisJson.Serialize(analysis));
        var output = document.RootElement.GetProperty("output");

        Assert.Multiple(() =>
        {
            Assert.That(output.GetProperty("type").GetString(), Is.EqualTo("grouping"));
            Assert.That(output.GetProperty("keys").GetProperty("type").GetString(), Is.EqualTo("union"));
            Assert.That(output.GetProperty("keys").GetProperty("alternatives").GetArrayLength(), Is.EqualTo(2));
            Assert.That(output.GetProperty("items").GetProperty("type").GetString(), Is.EqualTo("record"));
        });
    }

    [Test]
    public void Serialize_PairDictionaryAndOpenTuple_UsesExpandedShapes()
    {
        var analysis = new SchemaAnalysis(
            new PairLogicalSchema(
                new ScalarLogicalSchema("text"),
                new ScalarLogicalSchema("integer")),
            new DictionaryLogicalSchema(
                new ScalarLogicalSchema("text"),
                new TupleLogicalSchema(
                    [new ScalarLogicalSchema("integer")],
                    AdditionalItems: new ScalarLogicalSchema("numeric"))),
            SchemaAnalysisCompleteness.Known,
            [],
            []);

        using var document = JsonDocument.Parse(SchemaAnalysisJson.Serialize(analysis));
        var input = document.RootElement.GetProperty("input");
        var output = document.RootElement.GetProperty("output");
        var tuple = output.GetProperty("values");

        Assert.Multiple(() =>
        {
            Assert.That(input.GetProperty("type").GetString(), Is.EqualTo("pair"));
            Assert.That(input.GetProperty("key").GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(output.GetProperty("type").GetString(), Is.EqualTo("dictionary"));
            Assert.That(tuple.GetProperty("type").GetString(), Is.EqualTo("tuple"));
            Assert.That(tuple.GetProperty("items").GetArrayLength(), Is.EqualTo(1));
            Assert.That(tuple.GetProperty("additionalItems").GetProperty("type").GetString(),
                Is.EqualTo("numeric"));
        });
    }
}
