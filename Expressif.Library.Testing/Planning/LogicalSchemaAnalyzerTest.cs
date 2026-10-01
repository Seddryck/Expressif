using Expressif.Library.Composition;
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

    [TestCase("is-even", "numeric")]
    [TestCase("contains(\"x\")", "text")]
    [TestCase("is-today", "temporal")]
    public void Analyze_Predicate_UsesCanonicalInputAndBooleanOutput(
        string expression,
        string inputType)
    {
        var analysis = Analyze(expression);

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Input, Is.EqualTo(new ScalarLogicalSchema(inputType)));
            Assert.That(analysis.Output, Is.EqualTo(new ScalarLogicalSchema("boolean")));
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Known));
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
    public void Analyze_AccumulatorContracts_DeriveImplementationTypes()
    {
        var any = Analyze("fold(any)");
        var every = Analyze("fold(every)");
        var last = Analyze("{1, 2, 3} | fold(last)");
        var maximum = Analyze("{1, 2, 3} | fold(max)");
        var minimum = Analyze("{1, 2, 3} | fold(min)");
        var closest = Analyze("{1, 2, 3} | fold(closest(2))");
        var only = Analyze("{1, 2, 3} | fold(only(is-even, sum))");
        var reduce = Analyze("{1, 2, 3} | fold(reduce(add($0, $1)))");

        Assert.Multiple(() =>
        {
            Assert.That(any.Input,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("boolean"))));
            Assert.That(any.Output, Is.EqualTo(new ScalarLogicalSchema("boolean")));
            Assert.That(every.Input,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("boolean"))));
            Assert.That(every.Output, Is.EqualTo(new ScalarLogicalSchema("boolean")));
            Assert.That(last.Output, Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(maximum.Output, Is.EqualTo(new ScalarLogicalSchema("numeric", true)));
            Assert.That(minimum.Output, Is.EqualTo(new ScalarLogicalSchema("numeric", true)));
            Assert.That(closest.Output, Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(only.Output, Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(reduce.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Dynamic));
            Assert.That(reduce.Diagnostics, Has.One.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Code == "schema.dynamic"
                && diagnostic.Message.Contains("operation output", StringComparison.Ordinal)
                && diagnostic.Message.Contains("initial value", StringComparison.Ordinal)));
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

    [Test]
    public void Analyze_PairAndDictionary_PreserveComponentSchemas()
    {
        var pair = Analyze("pair(\"code\", 42)").Output as PairLogicalSchema
            ?? throw new AssertionException("Expected a pair schema.");
        var dictionary = Analyze("dictionary(pair(\"first\", 1), pair(\"second\", 2))").Output
            as DictionaryLogicalSchema
            ?? throw new AssertionException("Expected a dictionary schema.");

        Assert.Multiple(() =>
        {
            Assert.That(pair.Key, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(pair.Value, Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(dictionary.Keys, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(dictionary.Values, Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(Analyze("pair(\"code\", 42) | pair-value").Output,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
        });
    }

    [Test]
    public void Analyze_SortAndRankContracts_PreserveOriginalItemSchema()
    {
        var item = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["name"] = new(new ScalarLogicalSchema("text")),
            ["score"] = new(new ScalarLogicalSchema("decimal")),
        });
        var analysis = Analyze(
            "sort-by(.name -> :text) | rank-by(.score -> :integer)",
            new ArrayLogicalSchema(item));
        var sorted = AsRecord(AsArray(
            analysis.Nodes.Single(node => node.Path == "plan.items[0]").Output).Items);
        var ranked = analysis.Output as GroupingLogicalSchema
            ?? throw new AssertionException("Expected a grouping schema.");
        var rankedItem = AsRecord(ranked.Items);
        var criterion = analysis.Nodes.Single(
            node => node.Path == "plan.items[0].arguments[0].value");

        Assert.Multiple(() =>
        {
            Assert.That(sorted.Fields["name"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(sorted.Fields["score"].Schema, Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(ranked.Keys, Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(rankedItem.Fields.Keys, Is.EqualTo(sorted.Fields.Keys));
            Assert.That(rankedItem.Fields["name"], Is.EqualTo(sorted.Fields["name"]));
            Assert.That(rankedItem.Fields["score"], Is.EqualTo(sorted.Fields["score"]));
            Assert.That(criterion.Output, Is.EqualTo(new ScalarLogicalSchema("text", true)));
        });
    }

    [Test]
    public void Analyze_DenseRankByContract_PreservesOriginalItemSchema()
    {
        var item = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["name"] = new(new ScalarLogicalSchema("text")),
            ["score"] = new(new ScalarLogicalSchema("decimal")),
        });
        var analysis = Analyze("dense-rank-by(.score -> :integer)", new ArrayLogicalSchema(item));
        var ranked = analysis.Output as GroupingLogicalSchema
            ?? throw new AssertionException("Expected a grouping schema.");

        Assert.Multiple(() =>
        {
            Assert.That(ranked.Keys, Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(AsRecord(ranked.Items).Fields["name"].Schema,
                Is.EqualTo(item.Fields["name"].Schema));
            Assert.That(AsRecord(ranked.Items).Fields["score"].Schema,
                Is.EqualTo(item.Fields["score"].Schema));
        });
    }

    [Test]
    public void Analyze_SortTableContracts_PreserveOriginalRowSchema()
    {
        var item = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["name"] = new(new ScalarLogicalSchema("text")),
            ["score"] = new(new ScalarLogicalSchema("decimal")),
        });
        var pair = new PairLogicalSchema(new ScalarLogicalSchema("sort-key"), item);
        var table = Analyze("sort-table", new ArrayLogicalSchema(pair)).Output
            as SortTableLogicalSchema
            ?? throw new AssertionException("Expected a sort-table schema.");
        var arrayResults = new[] { "sort", "top(2)", "bottom(2)", "top-with-ties(2)", "bottom-with-ties(2)" }
            .Select(expression => Analyze(expression, table).Output)
            .ToArray();
        var groupingResults = new[] { "rank", "dense-rank" }
            .Select(expression => Analyze(expression, table).Output as GroupingLogicalSchema
                ?? throw new AssertionException("Expected a grouping schema."))
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(AsRecord(table.Items).Fields["name"], Is.EqualTo(item.Fields["name"]));
            Assert.That(AsRecord(table.Items).Fields["score"], Is.EqualTo(item.Fields["score"]));
            Assert.That(arrayResults, Has.All.Matches<ArrayLogicalSchema>(schema =>
                AsRecord(schema.Items).Fields.Keys.SequenceEqual(item.Fields.Keys)
                && AsRecord(schema.Items).Fields["name"].Schema == item.Fields["name"].Schema
                && AsRecord(schema.Items).Fields["score"].Schema == item.Fields["score"].Schema));
            Assert.That(groupingResults, Has.All.Matches<GroupingLogicalSchema>(schema =>
                schema.Keys == new ScalarLogicalSchema("integer")
                && AsRecord(schema.Items).Fields.Keys.SequenceEqual(item.Fields.Keys)
                && AsRecord(schema.Items).Fields["name"].Schema == item.Fields["name"].Schema
                && AsRecord(schema.Items).Fields["score"].Schema == item.Fields["score"].Schema));
        });
    }

    [Test]
    public void Analyze_SetContracts_DistinguishBothItemSources()
    {
        var input = new ArrayLogicalSchema(new ScalarLogicalSchema("decimal"));
        var complement = AsArray(Analyze("complement({\"x\"})", input).Output);
        var union = AsArray(Analyze("union({\"x\"})", input).Output).Items
            as UnionLogicalSchema
            ?? throw new AssertionException("Expected union items.");
        var symmetricDifference = AsArray(Analyze("symmetric-difference({\"x\"})", input).Output).Items
            as UnionLogicalSchema
            ?? throw new AssertionException("Expected union items.");

        Assert.Multiple(() =>
        {
            Assert.That(complement.Items, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(union.Alternatives, Is.EquivalentTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("decimal"),
                new ScalarLogicalSchema("text"),
            }));
            Assert.That(symmetricDifference.Alternatives, Is.EquivalentTo(union.Alternatives));
        });
    }

    [Test]
    public void Analyze_GroupingContracts_PreserveAndTransformComponents()
    {
        var key = new ScalarLogicalSchema("text");
        var item = new ScalarLogicalSchema("decimal");
        var grouping = new GroupingLogicalSchema(key, item);
        var grouped = Analyze("group", new ArrayLogicalSchema(new PairLogicalSchema(key, item))).Output
            as GroupingLogicalSchema
            ?? throw new AssertionException("Expected group to produce a grouping schema.");
        var preserved = new[]
        {
            "drop-empty-groups",
            "filter-groups($key | is-equivalent-to(\"BE\"))",
            "top-groups(1, $key)",
        }.Select(expression => Analyze(expression, grouping).Output).ToArray();
        var mapped = Analyze("map-groups(map(coerce-text))", grouping).Output
            as GroupingLogicalSchema
            ?? throw new AssertionException("Expected map-groups to produce a grouping schema.");
        var drilled = Analyze("drill-up(lower)", grouping).Output
            as GroupingLogicalSchema
            ?? throw new AssertionException("Expected drill-up to produce a grouping schema.");
        var summarized = Analyze("summarize-against(sum, sum, $0 | divide($1))", grouping).Output
            as DictionaryLogicalSchema
            ?? throw new AssertionException("Expected summarize-against to produce a dictionary schema.");

        Assert.Multiple(() =>
        {
            Assert.That((grouped.Keys, grouped.Items), Is.EqualTo((key, item)));
            Assert.That(preserved, Has.All.EqualTo(grouping));
            Assert.That((mapped.Keys, mapped.Items),
                Is.EqualTo((key, new ScalarLogicalSchema("text"))));
            Assert.That((drilled.Keys, drilled.Items), Is.EqualTo((key, item)));
            Assert.That((summarized.Keys, summarized.Values),
                Is.EqualTo((key, new ScalarLogicalSchema("numeric"))));
        });
    }

    [Test]
    public void Analyze_JoinContracts_PreserveSidesAndOuterNullability()
    {
        var left = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["id"] = new(new ScalarLogicalSchema("decimal")),
        });
        const string right = "{{id := 1, name := \"A\"}}";
        var inner = AsArray(Analyze($"join({right}, .id)", new ArrayLogicalSchema(left)).Output).Items
            as PairLogicalSchema
            ?? throw new AssertionException("Expected join to produce pairs.");
        var leftOuter = AsArray(Analyze($"join-left({right}, .id)", new ArrayLogicalSchema(left)).Output).Items
            as PairLogicalSchema
            ?? throw new AssertionException("Expected join-left to produce pairs.");
        var rightOuter = AsArray(Analyze($"join-right({right}, .id)", new ArrayLogicalSchema(left)).Output).Items
            as PairLogicalSchema
            ?? throw new AssertionException("Expected join-right to produce pairs.");
        var fullOuter = AsArray(Analyze($"join-full({right}, .id)", new ArrayLogicalSchema(left)).Output).Items
            as PairLogicalSchema
            ?? throw new AssertionException("Expected join-full to produce pairs.");
        var groupedRight = AsArray(Analyze("{\"A\"} | join({\"A\"} | group-by(@_), @_)").Output).Items
            as PairLogicalSchema
            ?? throw new AssertionException("Expected grouping join to produce pairs.");
        var dictionaryRight = AsArray(
            Analyze("{\"A\"} | join(dictionary(pair(\"A\", \"A\")), @_)").Output).Items
            as PairLogicalSchema
            ?? throw new AssertionException("Expected dictionary join to produce pairs.");
        Assert.Multiple(() =>
        {
            Assert.That(inner.Key, Is.TypeOf<RecordLogicalSchema>());
            Assert.That(inner.Value, Is.TypeOf<RecordLogicalSchema>());
            Assert.That(leftOuter.Key, Is.TypeOf<RecordLogicalSchema>());
            Assert.That(AsRecord(leftOuter.Value).IsNullable, Is.True);
            Assert.That(AsRecord(leftOuter.Value).Fields.Keys,
                Is.EqualTo(AsRecord(inner.Value).Fields.Keys));
            Assert.That(AsRecord(rightOuter.Key).IsNullable, Is.True);
            Assert.That(AsRecord(rightOuter.Key).Fields.Keys,
                Is.EqualTo(AsRecord(inner.Key).Fields.Keys));
            Assert.That(rightOuter.Value, Is.TypeOf<RecordLogicalSchema>());
            Assert.That(AsRecord(fullOuter.Key).IsNullable, Is.True);
            Assert.That(AsRecord(fullOuter.Value).IsNullable, Is.True);
            Assert.That(groupedRight.Value, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(dictionaryRight.Value, Is.EqualTo(new ScalarLogicalSchema("text")));
        });
    }

    [Test]
    public void Analyze_TuplePositionIntrinsic_SelectsKnownComponent()
    {
        var tuple = new TupleLogicalSchema(new LogicalSchema[]
        {
            new ScalarLogicalSchema("text"),
            new ScalarLogicalSchema("decimal"),
        });
        var pair = new PairLogicalSchema(
            new ScalarLogicalSchema("text"),
            new ScalarLogicalSchema("integer"));

        Assert.Multiple(() =>
        {
            Assert.That(Analyze("tuple-first", tuple).Output,
                Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(Analyze("tuple-second", tuple).Output,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(Analyze("tuple-at(-1)", tuple).Output,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(Analyze("tuple-at(1)", pair).Output,
                Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(Analyze("tuple-at(3)", tuple).Output,
                Is.EqualTo(new AnyLogicalSchema(true)));
        });
    }

    [Test]
    public void Analyze_WithIntrinsic_UsesProjectionRecordAsBodyContext()
    {
        var analysis = Analyze("with(label := .name | upper, record(value := .label))");
        var input = AsRecord(analysis.Input);
        var output = AsRecord(analysis.Output);

        Assert.Multiple(() =>
        {
            Assert.That(input.Fields["name"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(output.Fields.Keys, Is.EqualTo(new[] { "value" }));
            Assert.That(output.Fields["value"].Schema, Is.EqualTo(new ScalarLogicalSchema("text", true)));
            Assert.That(analysis.Diagnostics, Has.None.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Message.Contains("temporary record", StringComparison.Ordinal)));
        });
    }

    [Test]
    public void Analyze_SummarizeContract_PreservesKeysAndSummaryShape()
    {
        var item = new RecordLogicalSchema(new Dictionary<string, LogicalSchemaField>
        {
            ["amount"] = new(new ScalarLogicalSchema("decimal")),
        });
        var input = new GroupingLogicalSchema(new ScalarLogicalSchema("text"), item);
        var output = Analyze(
            "summarize(record(total := map(.amount) | sum, count := cardinality))",
            input).Output
            as DictionaryLogicalSchema
            ?? throw new AssertionException("Expected a dictionary schema.");
        var summary = AsRecord(output.Values);

        Assert.Multiple(() =>
        {
            Assert.That(output.Keys, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(summary.Fields.Keys, Is.EqualTo(new[] { "count", "total" }));
            Assert.That(summary.Fields["total"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(summary.Fields["count"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("integer")));
        });
    }

    [Test]
    public void Analyze_RecordSpread_MergesKnownFieldsInEntryOrder()
    {
        var output = AsRecord(Analyze(
            "record(a := 1, ...{b := \"x\", a := #true}, c := 3)").Output);
        var mapped = AsArray(Analyze("""
            {{code := "BE", score := 1}}
            | group-by(.code)
            | summarize(record(score := map(.score) | sum))
            | map(record(code := $key, ...($value)))
            """).Output);
        var mappedRecord = AsRecord(mapped.Items);

        Assert.Multiple(() =>
        {
            Assert.That(output.Fields["a"].Schema, Is.EqualTo(new ScalarLogicalSchema("boolean")));
            Assert.That(output.Fields["b"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(output.Fields["c"].Schema, Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(output.AllowsAdditionalFields, Is.False);
            Assert.That(mappedRecord.Fields["code"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(mappedRecord.Fields["score"].Schema, Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(mappedRecord.AllowsAdditionalFields, Is.False);
        });
    }

    [Test]
    public void Analyze_Record_NestedAndDynamicContributionsRetainKnownShape()
    {
        var nested = AsRecord(Analyze("record(parent := record(child := 1))").Output);
        var nestedParent = AsRecord(nested.Fields["parent"].Schema);
        var plan = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse("record(value := 1)"));
        var record = (LogicalCall)plan.Pipeline.Items.Single();
        var nestedRecord = (LogicalCall)record.Arguments.Single().Value!;
        var entry = (LogicalCall)nestedRecord.Arguments.Single().Value!;
        var dynamicName = LogicalPlannerFactory.Create()
            .Build(ExpressionParser.Parse("upper"))
            .Pipeline.Items.Single();
        var dynamicEntry = entry with
        {
            Arguments = entry.Arguments.Select(argument => argument.Parameter.Name == "name"
                ? argument with { Value = dynamicName }
                : argument).ToArray(),
        };
        var dynamicNestedRecord = nestedRecord with
        {
            Arguments = [nestedRecord.Arguments.Single() with { Value = dynamicEntry }],
        };
        var dynamicPlan = plan with
        {
            Pipeline = new LogicalPipeline([
                record with
                {
                    Arguments = [record.Arguments.Single() with { Value = dynamicNestedRecord }],
                },
            ]),
        };

        var dynamic = LogicalSchemaAnalyzer.Analyze(dynamicPlan);
        var dynamicRecord = AsRecord(dynamic.Output);

        Assert.Multiple(() =>
        {
            Assert.That(nestedParent.Fields["child"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(dynamicRecord.Fields, Is.Empty);
            Assert.That(dynamicRecord.AllowsAdditionalFields, Is.True);
            Assert.That(dynamic.Diagnostics, Has.One.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Code == "schema.dynamic" && diagnostic.Path == "plan.items[0]"));
        });
    }

    [Test]
    public void Analyze_OpenEndedTuple_SeparatesKnownPrefixFromAdditionalItems()
    {
        var spread = Analyze("tuple(1, ...{\"a\", \"b\"})").Output as TupleLogicalSchema
            ?? throw new AssertionException("Expected a tuple schema.");
        var converted = Analyze("{1, 2} | to-tuple").Output as TupleLogicalSchema
            ?? throw new AssertionException("Expected a tuple schema.");

        Assert.Multiple(() =>
        {
            Assert.That(spread.Items, Is.EqualTo(new LogicalSchema[] { new ScalarLogicalSchema("decimal") }));
            Assert.That(spread.AdditionalItems, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(converted.Items, Is.Empty);
            Assert.That(converted.AdditionalItems, Is.EqualTo(new ScalarLogicalSchema("decimal")));
        });
    }

    [Test]
    public void Analyze_ConditionalNullability_ObservesConfiguredParameter()
    {
        var regular = AsArray(Analyze("{1} | zip({\"a\"})").Output);
        var nullable = AsArray(Analyze("{1} | zip(#null)").Output);

        Assert.Multiple(() =>
        {
            Assert.That(regular.IsNullable, Is.False);
            Assert.That(nullable.IsNullable, Is.True);
        });
    }

    [Test]
    public void Analyze_RecordMutationIntrinsics_DistinguishPresenceConditions()
    {
        var present = AsRecord(Analyze("{a := 1} | put-present(a := \"x\", b := #true)").Output);
        var absent = AsRecord(Analyze("{a := 1} | put-absent(a := \"x\", b := #true)").Output);

        Assert.Multiple(() =>
        {
            Assert.That(present.Fields["a"].Schema, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(present.Fields.ContainsKey("b"), Is.False);
            Assert.That(absent.Fields["a"].Schema, Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(absent.Fields["b"].Schema, Is.EqualTo(new ScalarLogicalSchema("boolean")));
        });
    }

    [Test]
    public void Analyze_ImplementationBackedContracts_PreservePreciseSchemas()
    {
        var applied = Analyze("{name := \"Ada\"} | apply(.name | upper)");
        var selected = Analyze("{2, 1} | min-by(neutral)").Output;
        var closest = AsArray(Analyze("closest-by(.score, 1)").Input);
        var closestItem = AsRecord(closest.Items);

        Assert.Multiple(() =>
        {
            Assert.That(applied.Output, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(selected, Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(closestItem.Fields["score"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(Analyze("\"value\" | neutral | throw(is-null)").Output,
                Is.EqualTo(new ScalarLogicalSchema("text")));
        });
    }

    [Test]
    public void Analyze_ArrayContracts_PreserveElementAndNestedShapes()
    {
        var lead = AsArray(Analyze("{1, 2, 3} | lead").Output);
        var pairwise = AsArray(Analyze("{1, 2, 3} | pairwise").Output);
        var pair = pairwise.Items as TupleLogicalSchema
            ?? throw new AssertionException("Expected pairwise to produce tuple items.");
        var positioned = AsArray(Analyze("{\"a\", \"b\"} | with-position").Output);
        var position = positioned.Items as TupleLogicalSchema
            ?? throw new AssertionException("Expected with-position to produce tuple items.");
        var distributed = AsArray(Analyze("{1, 2, 3} | distribute-condition(is-even)").Output);
        var adjacent = AsArray(Analyze("{1, 2, 3} | adjacent(add($0, $1))").Output);
        var chunked = Analyze("{1, 2, 3} | chunk-around(1)").Output as TupleLogicalSchema
            ?? throw new AssertionException("Expected chunk-around to produce a tuple.");

        Assert.Multiple(() =>
        {
            Assert.That(Analyze("{1, 2, 3} | first-elements(2) | distinct").Output,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("decimal"))));
            Assert.That(lead.Items, Is.EqualTo(new ScalarLogicalSchema("decimal", true)));
            Assert.That(pair.Items, Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("decimal"),
                new ScalarLogicalSchema("decimal"),
            }));
            Assert.That(position.Items, Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("integer"),
                new ScalarLogicalSchema("text"),
            }));
            Assert.That(distributed.Items,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("decimal"))));
            Assert.That(adjacent.Items, Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(chunked.Items, Is.EqualTo(new LogicalSchema[]
            {
                new ArrayLogicalSchema(new ScalarLogicalSchema("decimal")),
                new ScalarLogicalSchema("decimal"),
                new ArrayLogicalSchema(new ScalarLogicalSchema("decimal")),
            }));
            Assert.That(Analyze("{1, 2, 3} | scan(sum)").Output,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("numeric"))));
        });
    }

    [Test]
    public void Analyze_ExplodeIntrinsic_ReplacesSelectedArrayWithItsItemSchema()
    {
        var exploded = AsArray(Analyze("{id := 1, tags := {\"A\", \"B\"}} | explode(.tags)").Output);
        var explodedItem = AsRecord(exploded.Items);
        var outer = AsArray(Analyze("{id := 1, tags := {\"A\"}} | explode-outer(.tags)").Output);
        var outerItem = AsRecord(outer.Items);
        var selector = Analyze("{id := 1, tags := {\"A\"}} | explode(.tags)").Nodes
            .Single(node => node.Path == "plan.items[1].arguments[0].value.items[0]");
        var selectorInput = AsRecord(selector.Input);

        Assert.Multiple(() =>
        {
            Assert.That(explodedItem.Fields["id"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(explodedItem.Fields["tags"],
                Is.EqualTo(new LogicalSchemaField(new ScalarLogicalSchema("text"))));
            Assert.That(outerItem.Fields["tags"],
                Is.EqualTo(new LogicalSchemaField(new ScalarLogicalSchema("text", true))));
            Assert.That(selectorInput.AllowsAdditionalFields, Is.False);
            Assert.That(selectorInput.Fields["id"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(selectorInput.Fields["tags"].Schema,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("text"))));
            Assert.That(selector.Output,
                Is.EqualTo(new ArrayLogicalSchema(new ScalarLogicalSchema("text"))));
        });
    }

    [Test]
    public void Analyze_ImplodeIntrinsic_ReplacesSelectedValueWithArraySchema()
    {
        var analysis = Analyze("{{id := 1, tags := \"A\"}} | implode(.tags)");
        var imploded = AsArray(analysis.Output);
        var implodedItem = AsRecord(imploded.Items);
        var selected = implodedItem.Fields["tags"].Schema as ArrayLogicalSchema
            ?? throw new AssertionException("Expected the imploded field to have an array schema.");
        var selector = analysis.Nodes
            .Single(node => node.Path == "plan.items[1].arguments[0].value.items[0]");
        var selectorInput = AsRecord(selector.Input);

        Assert.Multiple(() =>
        {
            Assert.That(implodedItem.Fields["id"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("decimal")));
            Assert.That(selected.Items,
                Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(selectorInput.Fields["tags"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(selector.Output,
                Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Known));
        });
    }

    [Test]
    public void Analyze_ImplodeInnerIntrinsic_RemovesSelectedValueNullability()
    {
        var input = new ArrayLogicalSchema(
            new RecordLogicalSchema(
                new Dictionary<string, LogicalSchemaField>
                {
                    ["id"] = new(new ScalarLogicalSchema("integer")),
                    ["tags"] = new(new ScalarLogicalSchema("text", true)),
                },
                AllowsAdditionalFields: false));
        var analysis = Analyze("implode-inner(.tags)", input);
        var imploded = AsArray(analysis.Output);
        var implodedItem = AsRecord(imploded.Items);
        var selected = implodedItem.Fields["tags"].Schema as ArrayLogicalSchema
            ?? throw new AssertionException("Expected the imploded field to have an array schema.");

        Assert.Multiple(() =>
        {
            Assert.That(implodedItem.Fields["id"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(selected.Items,
                Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Known));
        });
    }

    [Test]
    public void Analyze_ImplodeIntrinsic_PropagatesSelectedFieldRequirementBackward()
    {
        var analysis = Analyze("implode(.tags) | map(.tags | first | upper)");
        var input = AsArray(analysis.Input);
        var item = AsRecord(input.Items);

        Assert.Multiple(() =>
        {
            Assert.That(item.Fields["tags"].Optional, Is.True);
            Assert.That(item.Fields["tags"].Schema,
                Is.EqualTo(new AnyLogicalSchema(true)));
            Assert.That(analysis.Diagnostics, Has.None.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Path == "plan.items[0].arguments[0].value.items[0]"));
        });
    }

    [Test]
    public void Analyze_ExplodeIntrinsic_PropagatesSelectedFieldRequirementBackward()
    {
        var analysis = Analyze("""
            .boards
            | filter(.kind | equivalent-to("player"))
            | explode(.rows)
            | group-by(.rows.nick_name)
            """);
        var input = AsRecord(analysis.Input);
        var boards = AsArray(input.Fields["boards"].Schema);
        var board = AsRecord(boards.Items);
        var rows = AsArray(board.Fields["rows"].Schema);
        var row = AsRecord(rows.Items);

        Assert.Multiple(() =>
        {
            Assert.That(board.Fields["kind"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(board.Fields["rows"].Optional, Is.True);
            Assert.That(rows.IsNullable, Is.True);
            Assert.That(row.Fields.ContainsKey("nick_name"), Is.True);
            Assert.That(analysis.Diagnostics, Has.None.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Path == "plan.items[2].arguments[0].value.items[0]"));
        });
    }

    [Test]
    public void Analyze_PlayerRankingQuery_PreservesRowsThroughExplode()
    {
        var analysis = Analyze("""
            .boards
            | filter(.kind | equivalent-to("player"))
            | explode(.rows)
            | group-by(.rows.nick_name)
            | summarize(
                with(
                    best :=
                        sort-by(
                            .rows.rank -> :integer,
                            .name -> :text
                        )
                        | first(3)
                        | map(
                            record(
                                board := .name,
                                rank := .rows.rank
                            )
                        ),
                    record(
                        best-rankings := .best,
                        ranking-count := .best | cardinality,
                        rank-sum := .best | map(.rank) | sum
                    )
                )
            )
            | map(record(nickname := $key, ...($value)))
            | sort-by(.nickname -> :text)
            | rank-by(
                .ranking-count -> :integer | desc,
                .rank-sum -> :integer
            )
            | map(record(overall-rank := $key, players := $value))
            | explode(.players)
            | map(record(overall-rank := .overall-rank, ...(.players)))
            """);
        var input = AsRecord(analysis.Input);
        var board = AsRecord(AsArray(input.Fields["boards"].Schema).Items);
        var row = AsRecord(AsArray(board.Fields["rows"].Schema).Items);
        var explode = AsArray(analysis.Nodes.Single(node => node.Path == "plan.items[2]").Output);
        var explodedBoard = AsRecord(explode.Items);
        var summarized = analysis.Nodes.Single(node => node.Path == "plan.items[4]").Output
            as DictionaryLogicalSchema
            ?? throw new AssertionException("Expected summarize to produce a dictionary schema.");
        var summary = AsRecord(summarized.Values);
        var mappedSummary = AsRecord(AsArray(
            analysis.Nodes.Single(node => node.Path == "plan.items[5]").Output).Items);
        var sortedSummary = AsRecord(AsArray(
            analysis.Nodes.Single(node => node.Path == "plan.items[6]").Output).Items);
        var rankedSummary = analysis.Nodes.Single(node => node.Path == "plan.items[7]").Output
            as GroupingLogicalSchema
            ?? throw new AssertionException("Expected rank-by to produce a grouping schema.");
        var output = AsRecord(AsArray(analysis.Output).Items);

        Assert.Multiple(() =>
        {
            Assert.That(row.Fields.ContainsKey("nick_name"), Is.True);
            Assert.That(explodedBoard.Fields["rows"].Schema, Is.TypeOf<RecordLogicalSchema>());
            Assert.That(summary.Fields.Keys,
                Is.EqualTo(new[] { "best-rankings", "rank-sum", "ranking-count" }));
            Assert.That(summary.Fields["best-rankings"].Schema, Is.TypeOf<ArrayLogicalSchema>());
            Assert.That(summary.Fields["ranking-count"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(summary.Fields["rank-sum"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("numeric")));
            Assert.That(mappedSummary.Fields.Keys,
                Is.EqualTo(new[] { "best-rankings", "nickname", "rank-sum", "ranking-count" }));
            Assert.That(mappedSummary.AllowsAdditionalFields, Is.False);
            Assert.That(sortedSummary, Is.EqualTo(mappedSummary));
            Assert.That(rankedSummary.Keys, Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(AsRecord(rankedSummary.Items).Fields.Keys,
                Is.EqualTo(mappedSummary.Fields.Keys));
            Assert.That(output.Fields.Keys, Is.EqualTo(new[]
            {
                "best-rankings", "nickname", "overall-rank", "rank-sum", "ranking-count",
            }));
            Assert.That(output.Fields["overall-rank"].Schema,
                Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(output.AllowsAdditionalFields, Is.False);
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Partial));
            Assert.That(analysis.Diagnostics, Has.None.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Path == "plan.items[2].arguments[0].value.items[0]"));
        });
    }

    [Test]
    public void Analyze_DynamicClassification_ReportsCatalogReason()
    {
        var analysis = Analyze("\"{}\" | parse-json");

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Completeness, Is.EqualTo(SchemaAnalysisCompleteness.Dynamic));
            Assert.That(analysis.Diagnostics, Has.One.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Code == "schema.dynamic"
                && diagnostic.Message.Contains("decoded JSON shape", StringComparison.Ordinal)));
        });
    }

    [Test]
    public void Analyze_NamedExpressionInvocation_UsesBoundaryContractsAndTraversesBody()
    {
        var identity = new LogicalCall(
            new PlannerFunctionDescriptor(
                "identity", "any", "any", Kind: "extension", Namespace: "system"),
            []);
        var definition = new LogicalNamedExpressionDefinition(
            "convert",
            new LogicalPipeline([identity]),
            InputContract: new LogicalTypeContract("integer"),
            OutputContract: new LogicalTypeContract("text"));
        var plan = new LogicalPlan(new LogicalPipeline([
            new LogicalNamedExpressionInvocation("convert", []),
        ]))
        { Definitions = [definition] };

        var analysis = LogicalSchemaAnalyzer.Analyze(plan);

        Assert.Multiple(() =>
        {
            Assert.That(analysis.Input, Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(analysis.Output, Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(analysis.Nodes, Has.One.Matches<SchemaAnalysisNode>(node =>
                node.Path == "definitions[0].body" && node.Kind == "pipeline"));
            Assert.That(analysis.Nodes, Has.One.Matches<SchemaAnalysisNode>(node =>
                node.Path == "plan.items[0]"
                && node.Kind == "named-expression-invocation"
                && node.Operator == "convert"));
        });
    }

    private static SchemaAnalysis Analyze(string expression, LogicalSchema? input = null)
        => LogicalSchemaAnalyzer.Analyze(
            LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(expression)),
            input);

    private static RecordLogicalSchema AsRecord(LogicalSchema schema)
        => schema as RecordLogicalSchema
            ?? throw new AssertionException($"Expected a record schema but found {schema.GetType().Name}.");

    private static ArrayLogicalSchema AsArray(LogicalSchema schema)
        => schema as ArrayLogicalSchema
            ?? throw new AssertionException($"Expected an array schema but found {schema.GetType().Name}.");
}
