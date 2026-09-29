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
}
