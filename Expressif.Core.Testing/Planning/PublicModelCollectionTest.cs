using Expressif.Bindings;
using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class PublicModelCollectionTest
{
    [Test]
    public void BindingModels_SnapshotListsAndEnumerateSourcesOnce()
    {
        var functions = new List<Function> { Function.FromParameters("trim", []) };
        var parameters = new SingleUseEnumerable<IParameter>([new LiteralParameter(1)]);
        var expression = new OpenExpression(functions);
        var function = Function.FromParameters("add", parameters);

        functions.Add(Function.FromParameters("upper", []));

        Assert.Multiple(() =>
        {
            Assert.That(expression.Members, Has.Count.EqualTo(1));
            Assert.That(function.Parameters, Is.EqualTo(new IParameter[] { new LiteralParameter(1) }));
            Assert.That(parameters.EnumerationCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void LogicalPlanAndNamedExpression_SnapshotLists()
    {
        var values = new List<LogicalValue> { new LogicalLiteral("decimal", 1m) };
        var parameters = new List<LogicalNamedExpressionParameter> { new("value") };
        var pipeline = new LogicalPipeline(values);
        var definition = new LogicalNamedExpressionDefinition("sample", pipeline, parameters);
        var plan = new LogicalPlan(pipeline) { Definitions = [definition] };

        values.Add(new LogicalLiteral("decimal", 2m));
        parameters.Add(new LogicalNamedExpressionParameter("other"));

        Assert.Multiple(() =>
        {
            Assert.That(plan.Pipeline.Items, Has.Count.EqualTo(1));
            Assert.That(plan.Definitions.Single().Parameters, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void LogicalSchemaAndAnalysis_SnapshotListsAndDictionaries()
    {
        var fields = new Dictionary<string, LogicalSchemaField>
        {
            ["name"] = new(new ScalarLogicalSchema("text")),
        };
        var diagnostics = new List<SchemaAnalysisDiagnostic> { new("TEST", "$", "message") };
        var nodes = new List<SchemaAnalysisNode>();
        var schema = new RecordLogicalSchema(fields);
        var analysis = new SchemaAnalysis(schema, schema, SchemaAnalysisCompleteness.Known, diagnostics, nodes);

        fields.Add("age", new LogicalSchemaField(new ScalarLogicalSchema("integer")));
        diagnostics.Clear();
        nodes.Add(new SchemaAnalysisNode("$", "literal", null, schema, schema));

        Assert.Multiple(() =>
        {
            Assert.That(schema.Fields.Keys, Is.EqualTo(new[] { "name" }));
            Assert.That(analysis.Diagnostics, Has.Count.EqualTo(1));
            Assert.That(analysis.Nodes, Is.Empty);
        });
    }

    [Test]
    public void CollectionBearingRecords_UseStructuralEqualityAndHashCodes()
    {
        var leftPipeline = new LogicalPipeline([new LogicalLiteral("decimal", 1m)]);
        var rightPipeline = new LogicalPipeline([new LogicalLiteral("decimal", 1m)]);
        var leftSchema = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["value"] = new(new ScalarLogicalSchema("numeric")),
        });
        var rightSchema = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["value"] = new(new ScalarLogicalSchema("numeric")),
        });

        Assert.Multiple(() =>
        {
            Assert.That(leftPipeline, Is.EqualTo(rightPipeline));
            Assert.That(leftPipeline.GetHashCode(), Is.EqualTo(rightPipeline.GetHashCode()));
            Assert.That(leftSchema, Is.EqualTo(rightSchema));
            Assert.That(leftSchema.GetHashCode(), Is.EqualTo(rightSchema.GetHashCode()));
        });
    }

    [Test]
    public void LogicalPlan_JsonRoundTripPreservesStructuralEquality()
    {
        var plan = new LogicalPlan(new LogicalPipeline([new LogicalLiteral("decimal", 1m)]));
        var json = LogicalPlanJson.Serialize(plan);
        var roundTripped = LogicalPlanJson.Deserialize(json);

        Assert.Multiple(() =>
        {
            Assert.That(roundTripped, Is.EqualTo(plan));
            Assert.That(roundTripped.GetHashCode(), Is.EqualTo(plan.GetHashCode()));
        });
    }

    private sealed class SingleUseEnumerable<T>(IEnumerable<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
                throw new InvalidOperationException("The source was enumerated more than once.");
            return values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
