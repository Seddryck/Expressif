using System.Text.Json;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class AnalyzedLogicalPlanJsonTest
{
    [Test]
    public void Serialize_AttachesAnnotationsUsingLogicalPlanPaths()
    {
        var plan = LogicalPlanner.Plan(ExpressionParser.Parse("upper | first-chars(5)"));
        var analyzed = LogicalSchemaAnalyzer.AnalyzePlan(plan);

        using var document = JsonDocument.Parse(AnalyzedLogicalPlanJson.Serialize(analyzed));
        var root = document.RootElement;
        var pipeline = root.GetProperty("plan");
        var firstChars = pipeline.GetProperty("items")[1];
        var length = firstChars.GetProperty("arguments")[0].GetProperty("value");

        Assert.Multiple(() =>
        {
            Assert.That(root.GetProperty("format").GetString(),
                Is.EqualTo(AnalyzedLogicalPlanJson.FormatName));
            Assert.That(root.GetProperty("version").GetInt32(),
                Is.EqualTo(AnalyzedLogicalPlanJson.FormatVersion));
            Assert.That(root.GetProperty("completeness").GetString(), Is.EqualTo("known"));
            Assert.That(pipeline.GetProperty("input").GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(firstChars.GetProperty("operator").GetProperty("name").GetString(),
                Is.EqualTo("first-chars"));
            Assert.That(firstChars.GetProperty("output").GetProperty("type").GetString(),
                Is.EqualTo("text"));
            Assert.That(length.GetProperty("input").GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(length.GetProperty("output").GetProperty("type").GetString(), Is.EqualTo("decimal"));
        });
    }
}
