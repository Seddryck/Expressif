using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class LogicalSchemaAnalyzerTest
{
    [Test]
    public void Analyze_NestedFieldPipeline_DiscoversInputPathAndOutputType()
    {
        var analysis = Analyze(".customer.name | upper");

        var input = AsRecord(analysis.Input);
        var customer = AsRecord(input.Fields["customer"].Schema);
        Assert.Multiple(() =>
        {
            Assert.That(input.Fields["customer"].Optional, Is.True);
            Assert.That(customer.Fields["name"].Optional, Is.True);
            Assert.That(customer.Fields["name"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(analysis.Output, Is.EqualTo(new ScalarLogicalSchema("text", true)));
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Known));
        });
    }

    [Test]
    public void Analyze_Filter_DiscoversPredicateFieldOnArrayItems()
    {
        var analysis = Analyze("filter(.active)");

        var input = AsArray(analysis.Input);
        var item = AsRecord(input.Items);
        Assert.Multiple(() =>
        {
            Assert.That(item.Fields["active"].Schema, Is.EqualTo(new ScalarLogicalSchema("boolean")));
            Assert.That(item.Fields["active"].Optional, Is.True);
            Assert.That(analysis.Output, Is.EqualTo(analysis.Input));
        });
    }

    [Test]
    public void Analyze_Map_DiscoversIncomingAndEnclosingFieldTypes()
    {
        var analysis = Analyze("map(.price | multiply(.quantity))");

        var input = AsArray(analysis.Input);
        var item = AsRecord(input.Items);
        var output = AsArray(analysis.Output);
        Assert.Multiple(() =>
        {
            Assert.That(item.Fields.Keys, Is.EqualTo(new[] { "price", "quantity" }));
            Assert.That(item.Fields["price"].Schema, Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(item.Fields["quantity"].Schema, Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(output.Items, Is.EqualTo(new ScalarLogicalSchema("numeric", true)));
        });
    }

    [Test]
    public void Analyze_DeclaredInputConflict_ReportsDiagnostic()
    {
        var declared = new RecordLogicalSchema(
            new Dictionary<string, LogicalSchemaField>
            {
                ["name"] = new(new ScalarLogicalSchema("integer")),
            });

        var analysis = Analyze(".name | upper", declared);

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Conflicting));
            Assert.That(analysis.Diagnostics, Has.One
                .Property(nameof(SchemaAnalysisDiagnostic.Code)).EqualTo("schema.conflict"));
        });
    }

    [Test]
    public void Analyze_Pipeline_ReportsInputAndOutputForEachLogicalNode()
    {
        var analysis = Analyze(".customer | upper | length");

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Nodes.Select(node => (node.Path, node.Kind, node.Operator)), Is.EqualTo(new[]
            {
                ("plan", "pipeline", (string?)null),
                ("plan.items[0]", "call", "field"),
                ("plan.items[0].arguments[0].value", "literal", (string?)null),
                ("plan.items[1]", "call", "upper"),
                ("plan.items[2]", "call", "length"),
            }));
            Assert.That(analysis.Nodes.Single(node => node.Path == "plan.items[0]").Output,
                Is.EqualTo(new ScalarLogicalSchema("text", true)));
            Assert.That(analysis.Nodes.Single(node => node.Path == "plan.items[1]").Input,
                Is.EqualTo(new ScalarLogicalSchema("text", true)));
            Assert.That(analysis.Nodes.Single(node => node.Path == "plan.items[2]").Output,
                Is.EqualTo(new ScalarLogicalSchema("integer", true)));
        });
    }

    [Test]
    public void Analyze_ClosedPipeline_ReportsNoExternalInputAndKnownCompleteness()
    {
        var analysis = Analyze("{1, 2, 3} | map(add(1))");

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Input, Is.TypeOf<NoInputLogicalSchema>());
            Assert.That(analysis.Output, Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("numeric"))));
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Known));
        });
    }

    [Test]
    public void Analyze_MetadataContracts_InferReusableStructuralOperators()
    {
        var zipped = AsArray(Analyze("{1, 2} | zip({\"a\", \"b\"})").Output);
        var zippedItem = zipped.Items as TupleLogicalSchema
            ?? throw new AssertionException($"Expected a tuple schema but found {zipped.Items.GetType().Name}.");

        Assert.Multiple(() =>
        {
            Assert.That(Analyze("{1, 2} | reverse").Output,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("decimal"))));
            Assert.That(Analyze("{1, 2} | chunk(1)").Output,
                Is.EqualTo(new ArrayLogicalSchema(
                    new ArrayLogicalSchema(new ScalarLogicalSchema("decimal")))));
            Assert.That(Analyze("{1, 2} | single").Output,
                Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(zippedItem.Items, Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("decimal"),
                new ScalarLogicalSchema("text"),
            }));
        });
    }

    [Test]
    public void Analyze_RepresentativeContracts_ExposeNestedAndNullableShapes()
    {
        var lag = AsArray(Analyze("{1, 2, 3} | lag").Output);
        var zipped = AsArray(Analyze("{1, 2, 3} | zip-padded({\"a\", \"b\"})").Output);
        var zippedItem = zipped.Items as TupleLogicalSchema
            ?? throw new AssertionException($"Expected a tuple schema but found {zipped.Items.GetType().Name}.");

        Assert.Multiple(() =>
        {
            Assert.That(lag.Items, Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(zippedItem.Items[0], Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(zippedItem.Items[1], Is.EqualTo(new ScalarLogicalSchema("text", true)));
        });
    }

    [Test]
    public void Analyze_Coalesce_UnionsCandidateOutputs()
    {
        var declared = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["number"] = new(new ScalarLogicalSchema("integer")),
            ["label"] = new(new ScalarLogicalSchema("text")),
        });

        var output = Analyze("coalesce(.number, .label)", declared).Output as UnionLogicalSchema
            ?? throw new AssertionException("Expected a union schema.");

        Assert.Multiple(() =>
        {
            Assert.That(output.IsNullable, Is.True);
            Assert.That(output.Alternatives, Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("integer"),
                new ScalarLogicalSchema("text"),
            }));
        });
    }

    [Test]
    public void Analyze_SelectFields_ProjectsLiteralFieldNames()
    {
        var analysis = Analyze("{name := \"Ada\", age := 42} | select-fields({\"age\"})");
        var output = AsRecord(analysis.Output);

        Assert.Multiple(() =>
        {
            Assert.That(output.Fields.Keys, Is.EqualTo(new[] { "age" }));
            Assert.That(output.Fields["age"].Schema, Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(output.AllowsAdditionalFields, Is.False);
        });
    }

    [Test]
    public void Analyze_Fold_DerivesResultFromAccumulatorMetadata()
    {
        var analysis = Analyze("{1, 2, 3} | fold(sum)");

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Input, Is.TypeOf<NoInputLogicalSchema>());
            Assert.That(analysis.Output, Is.EqualTo(new ScalarLogicalSchema("numeric")));
        });
    }

    [Test]
    public void Analyze_GroupBy_CapturesKeyAndItemSchemas()
    {
        var analysis = Analyze("{{country := \"BE\", year := 2025}} | group-by(.country, .year)");
        var grouping = analysis.Output as GroupingLogicalSchema
            ?? throw new AssertionException("Expected a grouping schema.");
        var keys = grouping.Keys as TupleLogicalSchema
            ?? throw new AssertionException("Expected tuple grouping keys.");
        var items = AsRecord(grouping.Items);

        Assert.Multiple(() =>
        {
            Assert.That(keys.Items, Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("text"),
                new ScalarLogicalSchema("decimal"),
            }));
            Assert.That(items.Fields.Keys, Is.EqualTo(new[] { "country", "year" }));
        });
    }

    [Test]
    public void Analyze_RepresentativeContracts_PropagateRequirementsBackward()
    {
        var folded = AsArray(Analyze("fold(sum)").Input);
        var selected = AsRecord(Analyze("select-fields({\"name\"}) | .name | upper").Input);

        Assert.Multiple(() =>
        {
            Assert.That(folded.Items, Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(selected.Fields["name"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
        });
    }

    private static SchemaAnalysis Analyze(string expression, LogicalSchema? input = null)
        => LogicalSchemaAnalyzer.Analyze(LogicalPlanner.Plan(ExpressionParser.Parse(expression)), input);

    private static RecordLogicalSchema AsRecord(LogicalSchema schema)
        => schema as RecordLogicalSchema
            ?? throw new AssertionException($"Expected a record schema but found {schema.GetType().Name}.");

    private static ArrayLogicalSchema AsArray(LogicalSchema schema)
        => schema as ArrayLogicalSchema
            ?? throw new AssertionException($"Expected an array schema but found {schema.GetType().Name}.");
}
