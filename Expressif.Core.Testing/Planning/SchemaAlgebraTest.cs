using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class SchemaAlgebraTest
{
    [Test]
    public void Intersect_SpecialSchemas_PreservesIdentityAndNullability()
    {
        var algebra = new SchemaAlgebra([]);
        var noInput = new NoInputLogicalSchema();
        var scalar = new ScalarLogicalSchema("text");

        Assert.Multiple(() =>
        {
            Assert.That(algebra.Intersect(noInput, new AnyLogicalSchema(), "root"), Is.SameAs(noInput));
            Assert.That(algebra.Intersect(new AnyLogicalSchema(), noInput, "root"), Is.SameAs(noInput));
            Assert.That(
                algebra.Intersect(new AnyLogicalSchema(IsNullable: true), scalar, "root"),
                Is.EqualTo(new ScalarLogicalSchema("text", IsNullable: true)));
        });
    }

    [Test]
    public void Intersect_Union_FiltersAlternativesAndPreservesNullability()
    {
        var diagnostics = new List<SchemaAnalysisDiagnostic>();
        var algebra = new SchemaAlgebra(diagnostics);
        var union = new UnionLogicalSchema(
            [new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")],
            IsNullable: true);

        var result = algebra.Intersect(union, new ScalarLogicalSchema("numeric"), "root");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new ScalarLogicalSchema("integer", IsNullable: true)));
            Assert.That(diagnostics, Is.Empty);
        });
    }

    [Test]
    public void Intersect_IncompatibleRoots_ReturnsConflictAndDiagnostic()
    {
        var diagnostics = new List<SchemaAnalysisDiagnostic>();
        var algebra = new SchemaAlgebra(diagnostics);
        var left = new ScalarLogicalSchema("text");
        var right = new ArrayLogicalSchema(new AnyLogicalSchema());

        var result = algebra.Intersect(left, right, "root");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new ConflictingLogicalSchema(left, right)));
            Assert.That(diagnostics, Has.One.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Code == "schema.conflict" && diagnostic.Path == "root"));
        });
    }

    [Test]
    public void Intersect_IncompatibleTupleLengths_ReturnsConflict()
    {
        var diagnostics = new List<SchemaAnalysisDiagnostic>();
        var algebra = new SchemaAlgebra(diagnostics);
        var left = new TupleLogicalSchema(
            [new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")]);
        var right = new TupleLogicalSchema([new ScalarLogicalSchema("text")]);

        var result = algebra.Intersect(left, right, "root");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new ConflictingLogicalSchema(left, right)));
            Assert.That(diagnostics, Has.One.Matches<SchemaAnalysisDiagnostic>(diagnostic =>
                diagnostic.Code == "schema.conflict" && diagnostic.Path == "root"));
        });
    }

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
        var result = SchemaAlgebra.Union(
            new ScalarLogicalSchema("integer"),
            new ScalarLogicalSchema("decimal", IsNullable: true));

        Assert.That(result, Is.EqualTo(new ScalarLogicalSchema("numeric", IsNullable: true)));
    }

    [Test]
    public void Union_SpecialSchemas_PreservesKnownShapeAndNullability()
    {
        var scalar = new ScalarLogicalSchema("text");

        Assert.Multiple(() =>
        {
            Assert.That(SchemaAlgebra.Union(new AnyLogicalSchema(), scalar), Is.TypeOf<AnyLogicalSchema>());
            Assert.That(
                SchemaAlgebra.Union(new AnyLogicalSchema(IsNullable: true), scalar),
                Is.EqualTo(new ScalarLogicalSchema("text", IsNullable: true)));
            Assert.That(
                SchemaAlgebra.Union(scalar, new AnyLogicalSchema(IsNullable: true)),
                Is.EqualTo(new ScalarLogicalSchema("text", IsNullable: true)));
            Assert.That(SchemaAlgebra.Union(scalar, scalar), Is.SameAs(scalar));
        });
    }

    [Test]
    public void Union_TemporalScalars_NormalizesToTemporal()
    {
        var result = SchemaAlgebra.Union(
            new ScalarLogicalSchema("date"),
            new ScalarLogicalSchema("datetime"));

        Assert.That(result, Is.EqualTo(new ScalarLogicalSchema("temporal")));
    }

    [Test]
    public void Union_DistinctShapes_FlattensAndDeduplicatesAlternatives()
    {
        var text = new ScalarLogicalSchema("text");
        var integer = new ScalarLogicalSchema("integer");
        var initial = new UnionLogicalSchema([text, integer], IsNullable: true);

        var result = (UnionLogicalSchema)SchemaAlgebra.Union(initial, text);

        Assert.Multiple(() =>
        {
            Assert.That(result.Alternatives, Is.EqualTo(new LogicalSchema[] { text, integer }));
            Assert.That(result.IsNullable, Is.True);
        });
    }

    [Test]
    public void Intersect_VariadicTuples_UsesAdditionalItemsForMissingPositions()
    {
        var algebra = new SchemaAlgebra([]);
        var left = new TupleLogicalSchema(
            [new ScalarLogicalSchema("text")],
            AdditionalItems: new ScalarLogicalSchema("numeric"));
        var right = new TupleLogicalSchema(
            [new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")]);

        var result = (TupleLogicalSchema)algebra.Intersect(left, right, "root");

        Assert.That(
            result.Items,
            Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("text"),
                new ScalarLogicalSchema("integer"),
            }));
    }

    [Test]
    public void Intersect_CompositeSchemas_IntersectsTheirChildren()
    {
        var algebra = new SchemaAlgebra([]);
        var integer = new ScalarLogicalSchema("integer");
        var numeric = new ScalarLogicalSchema("numeric");

        var results = new LogicalSchema[]
        {
            algebra.Intersect(new ArrayLogicalSchema(integer), new ArrayLogicalSchema(numeric), "array"),
            algebra.Intersect(new PairLogicalSchema(integer, integer), new PairLogicalSchema(numeric, numeric), "pair"),
            algebra.Intersect(
                new DictionaryLogicalSchema(integer, integer),
                new DictionaryLogicalSchema(numeric, numeric),
                "dictionary"),
            algebra.Intersect(
                new GroupingLogicalSchema(integer, integer),
                new GroupingLogicalSchema(numeric, numeric),
                "grouping"),
            algebra.Intersect(
                new SortTableLogicalSchema(integer),
                new SortTableLogicalSchema(numeric),
                "sort-table"),
        };

        Assert.That(results, Has.All.Matches<LogicalSchema>(schema => !SchemaAlgebra.ContainsConflict(schema)));
    }

    [Test]
    public void SchemaInspection_HandlesEveryCompositeShape()
    {
        var any = new AnyLogicalSchema();
        var conflict = new ConflictingLogicalSchema(
            new ScalarLogicalSchema("text"),
            new ScalarLogicalSchema("integer"));
        var schemasWithAny = new LogicalSchema[]
        {
            Record(("value", any, false)),
            new ArrayLogicalSchema(any),
            new TupleLogicalSchema([any]),
            new TupleLogicalSchema([], AdditionalItems: any),
            new PairLogicalSchema(any, any),
            new DictionaryLogicalSchema(any, any),
            new GroupingLogicalSchema(any, any),
            new SortTableLogicalSchema(any),
            new UnionLogicalSchema([any, new ScalarLogicalSchema("text")]),
            new ConflictingLogicalSchema(any, new ScalarLogicalSchema("text")),
        };
        var schemasWithConflict = new LogicalSchema[]
        {
            Record(("value", conflict, false)),
            new ArrayLogicalSchema(conflict),
            new TupleLogicalSchema([conflict]),
            new TupleLogicalSchema([], AdditionalItems: conflict),
            new PairLogicalSchema(conflict, conflict),
            new DictionaryLogicalSchema(conflict, conflict),
            new GroupingLogicalSchema(conflict, conflict),
            new SortTableLogicalSchema(conflict),
            new UnionLogicalSchema([conflict, new ScalarLogicalSchema("text")]),
        };

        Assert.Multiple(() =>
        {
            Assert.That(schemasWithAny, Has.All.Matches<LogicalSchema>(SchemaAlgebra.ContainsAny));
            Assert.That(schemasWithConflict, Has.All.Matches<LogicalSchema>(SchemaAlgebra.ContainsConflict));
            Assert.That(SchemaAlgebra.ContainsAny(new ScalarLogicalSchema("text")), Is.False);
            Assert.That(SchemaAlgebra.ContainsConflict(new ScalarLogicalSchema("text")), Is.False);
        });
    }

    [Test]
    public void WithNullability_AndDescribe_HandleEverySchemaShape()
    {
        var schemas = new (LogicalSchema Schema, string Description)[]
        {
            (new AnyLogicalSchema(), "any"),
            (new ScalarLogicalSchema("text"), "text"),
            (Record(("value", new ScalarLogicalSchema("text"), false)), "record"),
            (new ArrayLogicalSchema(new AnyLogicalSchema()), "array"),
            (new TupleLogicalSchema([]), "tuple"),
            (new PairLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()), "pair"),
            (new DictionaryLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()), "dictionary"),
            (new GroupingLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()), "grouping"),
            (new SortTableLogicalSchema(new AnyLogicalSchema()), "sort-table"),
            (new UnionLogicalSchema([new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")]), "union"),
            (new ConflictingLogicalSchema(new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")), "conflict"),
        };

        Assert.Multiple(() =>
        {
            foreach (var (schema, description) in schemas)
            {
                var nullable = SchemaAlgebra.WithNullability(schema, true);
                Assert.That(SchemaAlgebra.IsNullable(nullable), Is.True, description);
                Assert.That(SchemaAlgebra.Describe(nullable), Is.EqualTo(description));
            }
            Assert.That(SchemaAlgebra.IsNullable(new NoInputLogicalSchema()), Is.False);
            Assert.That(SchemaAlgebra.Describe(new NoInputLogicalSchema()), Is.EqualTo("no-input"));
        });
    }

    [Test]
    public void IntersectScalar_HandlesScalarNumericTemporalAndIncompatibleTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SchemaAlgebra.IntersectScalar("text", "text"), Is.EqualTo("text"));
            Assert.That(SchemaAlgebra.IntersectScalar("scalar", "text"), Is.EqualTo("text"));
            Assert.That(SchemaAlgebra.IntersectScalar("text", "scalar"), Is.EqualTo("text"));
            Assert.That(SchemaAlgebra.IntersectScalar("numeric", "integer"), Is.EqualTo("integer"));
            Assert.That(SchemaAlgebra.IntersectScalar("decimal", "numeric"), Is.EqualTo("decimal"));
            Assert.That(SchemaAlgebra.IntersectScalar("temporal", "date"), Is.EqualTo("date"));
            Assert.That(SchemaAlgebra.IntersectScalar("time", "temporal"), Is.EqualTo("time"));
            Assert.That(SchemaAlgebra.IntersectScalar("text", "integer"), Is.Null);
        });
    }

    [Test]
    public void RecordContributionFold_PreservesOrderAndOptionalFieldSemantics()
    {
        IRecordSchemaContribution[] contributions =
        [
            new NamedRecordContribution("value", new LogicalSchemaField(new ScalarLogicalSchema("decimal"))),
            new RecordShapeContribution(Record(
                ("value", new ScalarLogicalSchema("text"), true),
                ("nested", Record(("flag", new ScalarLogicalSchema("boolean"), false)), false))),
            new NamedRecordContribution("last", new LogicalSchemaField(new ScalarLogicalSchema("date"))),
        ];

        var result = RecordSchemaContributionFold.Apply(contributions);

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
        var open = Record(("known", new ScalarLogicalSchema("text"), false))
            with
        { AllowsAdditionalFields = true };

        var spread = RecordSchemaContributionFold.Apply([new RecordShapeContribution(open)]);
        var dynamic = RecordSchemaContributionFold.Apply([DynamicRecordContribution.Instance]);

        Assert.Multiple(() =>
        {
            Assert.That(spread.AllowsAdditionalFields, Is.True);
            Assert.That(spread.Fields.ContainsKey("known"), Is.True);
            Assert.That(dynamic.AllowsAdditionalFields, Is.True);
        });
    }

    [Test]
    public void RegisterIntrinsicRule_NewIntrinsic_AcceptsRule()
    {
        var session = new LogicalSchemaAnalysisSession([]);

        var action = () => session.RegisterIntrinsicRule(new TestIntrinsicRule("custom"));

        Assert.That(action, Throws.Nothing);
    }

    [Test]
    public void RegisterIntrinsicRule_DuplicateIntrinsic_Throws()
    {
        var session = new LogicalSchemaAnalysisSession([]);

        var action = () => session.RegisterIntrinsicRule(new TestIntrinsicRule("field"));

        Assert.That(action, Throws.InvalidOperationException
            .With.Message.EqualTo("A schema rule is already registered for 'field'."));
    }

    private static RecordLogicalSchema Record(
        params (string Name, LogicalSchema Schema, bool Optional)[] fields)
        => new(fields.ToDictionary(
            field => field.Name,
            field => new LogicalSchemaField(field.Schema, field.Optional),
            StringComparer.Ordinal),
            AllowsAdditionalFields: false);

    private sealed class TestIntrinsicRule(string intrinsic)
        : LogicalSchemaAnalysisSession.IIntrinsicSchemaRule
    {
        public IReadOnlyCollection<string> Intrinsics { get; } = [intrinsic];

        public LogicalSchemaAnalysisSession.Requirement Require(
            LogicalCall call,
            LogicalSchema expected,
            string path,
            string intrinsic)
            => new(expected, new AnyLogicalSchema());

        public LogicalSchema Infer(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path,
            string intrinsic)
            => input;
    }
}
