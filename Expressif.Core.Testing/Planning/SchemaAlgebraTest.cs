using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class SchemaAlgebraTest
{
    [Test]
    public void Intersect_RecordConstraints_MergesFieldsAndReportsConflicts()
    {
        var diagnostics = new List<SchemaAnalysisDiagnostic>();
        var algebra = new SchemaAlgebra(diagnostics);
        var left = Record(("value", new ScalarLogicalSchema("text"), false));
        var right = Record(
            ("value", new ScalarLogicalSchema("decimal"), true),
            ("other", new ScalarLogicalSchema("boolean"), false));

        var result = (RecordLogicalSchema)algebra.Intersect(left, right, "root");

        Assert.Multiple(() =>
        {
            Assert.That(result.Fields.Keys, Is.EqualTo(new[] { "other", "value" }));
            Assert.That(result.Fields["value"].Optional, Is.True);
            Assert.That(result.Fields["value"].Schema, Is.TypeOf<ConflictingLogicalSchema>());
            Assert.That(diagnostics, Has.One.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Code == "schema.conflict" && diagnostic.Path == "root.value"));
        });
    }

    [Test]
    public void Union_NumericScalars_NormalizesToNumericAndPreservesNullability()
    {
        var algebra = new SchemaAlgebra([]);

        var result = algebra.Union(
            new ScalarLogicalSchema("integer"),
            new ScalarLogicalSchema("decimal", IsNullable: true));

        Assert.That(result, Is.EqualTo(new ScalarLogicalSchema("numeric", IsNullable: true)));
    }

    [Test]
    public void RecordContributionFold_PreservesOrderAndOptionalFieldSemantics()
    {
        var algebra = new SchemaAlgebra([]);
        RecordSchemaContribution[] contributions =
        [
            new NamedRecordContribution("value", new LogicalSchemaField(new ScalarLogicalSchema("decimal"))),
            new RecordShapeContribution(Record(
                ("value", new ScalarLogicalSchema("text"), true),
                ("nested", Record(("flag", new ScalarLogicalSchema("boolean"), false)), false))),
            new NamedRecordContribution("last", new LogicalSchemaField(new ScalarLogicalSchema("date"))),
        ];

        var result = RecordSchemaContributionFold.Apply(contributions, algebra);

        Assert.Multiple(() =>
        {
            Assert.That(result.Fields.Keys, Is.EqualTo(new[] { "last", "nested", "value" }));
            Assert.That(result.Fields["value"].Optional, Is.False);
            Assert.That(result.Fields["value"].Schema, Is.TypeOf<UnionLogicalSchema>());
            Assert.That(result.Fields["nested"].Schema, Is.TypeOf<RecordLogicalSchema>());
            Assert.That(result.AllowsAdditionalFields, Is.False);
        });
    }

    [Test]
    public void RecordContributionFold_OpenOrDynamicContribution_OpensShape()
    {
        var algebra = new SchemaAlgebra([]);
        var open = Record(("known", new ScalarLogicalSchema("text"), false))
            with
        { AllowsAdditionalFields = true };

        var spread = RecordSchemaContributionFold.Apply([new RecordShapeContribution(open)], algebra);
        var dynamic = RecordSchemaContributionFold.Apply([DynamicRecordContribution.Instance], algebra);

        Assert.Multiple(() =>
        {
            Assert.That(spread.AllowsAdditionalFields, Is.True);
            Assert.That(spread.Fields.ContainsKey("known"), Is.True);
            Assert.That(dynamic.AllowsAdditionalFields, Is.True);
        });
    }

    private static RecordLogicalSchema Record(
        params (string Name, LogicalSchema Schema, bool Optional)[] fields)
        => new(fields.ToDictionary(
            field => field.Name,
            field => new LogicalSchemaField(field.Schema, field.Optional),
            StringComparer.Ordinal),
            AllowsAdditionalFields: false);
}
