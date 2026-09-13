using Expressif.Semantics;

namespace Expressif.Testing.Semantics;

public class SemanticAnalyzerTest
{
    [TestCase("catch", SemanticSourceKind.ExternalInput)]
    [TestCase("throw", SemanticSourceKind.Expression)]
    public void Analyze_NullControlFlow_ArgumentSource(string name, SemanticSourceKind expected)
    {
        var reference = Analyze($".item | {name}(.name)").References.Last();
        Assert.That(reference.Source.Kind, Is.EqualTo(expected));
    }

    [TestCase(".name", FieldReferenceKind.CurrentInput)]
    [TestCase("^.name", FieldReferenceKind.ExpressionRoot)]
    public void Analyze_ExternalSource_HasNoDocumentSpan(string text, FieldReferenceKind kind)
    {
        var reference = Analyze(text).References.Single();
        Assert.That(reference.Kind, Is.EqualTo(kind));
        Assert.That(reference.Source.Kind, Is.EqualTo(SemanticSourceKind.ExternalInput));
        Assert.That(reference.Source.Span, Is.Null);
    }

    [Test]
    public void Analyze_MissingEnclosingRoot_IsUnresolved()
    {
        var source = Analyze("^^.name").References.Single().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Unresolved));
        Assert.That(source.Span, Is.Null);
    }

    [Test]
    public void Analyze_ChainedFields_EachSelectionReadsPreviousSelection()
    {
        const string text = "{a := {b := {c := 42}}} | .a.b.c";
        var references = Analyze(text).References;
        Assert.That(references.Select(reference => Slice(text, reference.Span)), Is.EqualTo(new[] { ".a", ".b", ".c" }));
        Assert.That(references.Select(reference => Slice(text, reference.Source.Span!.Value)),
            Is.EqualTo(new[] { "{a := {b := {c := 42}}}", ".a", ".a.b" }));
    }

    [Test]
    public void Analyze_PipelineRoot_DoesNotAdvanceWithCurrentInput()
    {
        const string text = "{a := {b := 42}, name := 7} | .a | .b | ^.name";
        var references = Analyze(text).References;
        Assert.That(Slice(text, references[1].Source.Span!.Value), Is.EqualTo(".a"));
        Assert.That(Slice(text, references[2].Source.Span!.Value), Is.EqualTo("{a := {b := 42}, name := 7}"));
        Assert.That(Expression.CreateClosed(text).Evaluate(null), Is.EqualTo(7m));
    }

    [Test]
    public void Analyze_MultilineReferences_PreservesCanonicalNodesAndBoundaries()
    {
        const string text = ".items |\r\n map(.value | upper | ^.field | suffix(^^.name))";
        var syntax = ExpressifSyntax.Parse(text);
        var result = new SemanticAnalyzer().Analyze(syntax);
        Assert.That(result.Syntax, Is.SameAs(syntax));
        Assert.That(result.References.Select(reference => Slice(text, reference.Span)),
            Is.EqualTo(new[] { ".items", ".value", "^.field", "^^.name" }));
        Assert.That(result.References[2].Source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(Slice(text, result.References[2].Source.Span!.Value), Is.EqualTo(".items"));
        Assert.That(result.References[3].Source.Kind, Is.EqualTo(SemanticSourceKind.ExternalInput));
    }

    [Test]
    public void Analyze_NestedMaps_EnclosingRootIsOuterItem()
    {
        const string text = ".groups | map(.items | map(.value | suffix(^^.name)))";
        var source = Analyze(text).References.Last().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(Slice(text, source.Span!.Value), Is.EqualTo(".groups"));
    }

    [Test]
    public void Analyze_FilterPipeline_RootIsFilteredItem()
    {
        const string text = "{{active := #true}, {active := #false}} | filter(^.active | is-true)";
        var source = Analyze(text).References.Single().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(source.Input!.Kind, Is.EqualTo(SemanticSourceKind.Expression));
        Assert.That(Expression.CreateClosed(text).Evaluate(null), Has.Length.EqualTo(1));
    }

    [Test]
    public void Analyze_WithAndSinglePredicate_EnclosingRootIsTemporaryRecord()
    {
        const string text = "with(values := {1, 2}, threshold := 1, .values | filter(greater-than(^^.threshold)))";
        var source = Analyze(text).References.Last().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Expression));
        Assert.That(source.Syntax!.Text, Is.EqualTo(text));
        Assert.That(Expression.Create(text).Evaluate(null), Is.EqualTo(new object[] { 2m }));
    }

    [TestCase("map-over", SemanticSourceKind.ExternalInput)]
    [TestCase("map-with", SemanticSourceKind.Element)]
    public void Analyze_DirectionalMap_SeparatesPipelineAndArgumentSources(string map, SemanticSourceKind inputKind)
    {
        var text = $"{map}(.name | suffix(^.suffix) | suffix(.suffix) | suffix(^^.name), {{{{suffix := \"!\", name := \"item\"}}}})";
        var references = Analyze(text).References;
        Assert.That(references[0].Source.Kind, Is.EqualTo(inputKind));
        Assert.That(references[1].Source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(references[2].Source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(references[3].Source.Kind, Is.EqualTo(SemanticSourceKind.ExternalInput));
    }

    [TestCase("map-over", "outer!")]
    [TestCase("map-with", "item!")]
    public void Analyze_DirectionalMaps_AgreeWithRuntime(string map, string expected)
    {
        var text = $"{map}(.name | suffix(^.suffix), {{{{name := \"item\", suffix := \"!\"}}}})";
        var input = new Dictionary<string, object?> { ["name"] = "outer", ["suffix"] = "wrong" };
        Assert.That(Analyze(text).References.Last().Source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(Expression.Create(text).Evaluate(input), Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Analyze_NamedDirectionalArguments_ResolvesCanonicalParameterOrder()
    {
        const string text = "map-over(values := {{x := 1}}, expression := .x | add(^.x))";
        Assert.That(Analyze(text).References.Select(reference => reference.Source.Kind),
            Is.EqualTo(new[] { SemanticSourceKind.ExternalInput, SemanticSourceKind.Element }));
    }

    [Test]
    public void Analyze_ChainedRootArgument_PreservesEverySelection()
    {
        const string text = ".items | map(add(^^.config.amount))";
        var references = Analyze(text).References;
        Assert.That(references.Select(reference => Slice(text, reference.Span)), Is.EqualTo(new[] { ".items", "^^.config", ".amount" }));
        Assert.That(references[1].Source.Kind, Is.EqualTo(SemanticSourceKind.ExternalInput));
        Assert.That(Slice(text, references[2].Source.Span!.Value), Is.EqualTo("^^.config"));
    }

    [Test]
    public void Analyze_EnclosingRootReferencesInScalarPipelines_UseSameOuterSource()
    {
        const string text = """
            {taxRate := 0.20, prices := {100, 200, 50}}
            | .prices
            | map(multiply((^^.taxRate | add(1) | add(^^.prices | cardinality))))
            """;
        var references = Analyze(text).References
            .Where(reference => reference.Kind == FieldReferenceKind.EnclosingExpressionRoot)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(references, Has.Length.EqualTo(2));
            Assert.That(references.Select(reference => reference.Source),
                Is.All.EqualTo(references[0].Source));
            Assert.That(Expression.Create(text).Evaluate(null),
                Is.EqualTo(new object?[] { 420m, 840m, 210m }));
        });
    }

    [Test]
    public void Analyze_VariableInput_DoesNotRequireProvider()
    {
        var source = Analyze("@missing | .name | divide(0)").References.Single().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.ExternalInput));
        Assert.That(source.Span, Is.Null);
    }

    [Test]
    public void Analyze_InvalidSyntax_ReturnsDiagnostics()
    {
        var result = new SemanticAnalyzer().Analyze("map(.foo |");
        Assert.That(result.Diagnostics, Is.Not.Empty);
        Assert.That(result.References, Is.Empty);
    }

    [TestCase("unknown(.name)")]
    [TestCase("unknown | .name")]
    public void Analyze_UnsupportedOperation_PreservesUnresolvedReferences(string text)
    {
        var source = Analyze(text).References.Single().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Unresolved));
        Assert.That(source.Span, Is.Null);
    }

    [Test]
    public void Analyze_InvalidBinding_PreservesUnresolvedReferences()
    {
        var result = new SemanticAnalyzer().Analyze("map(extra := .name)");
        Assert.That(result.Diagnostics, Is.Not.Empty);
        Assert.That(result.References.Single().Source.Kind, Is.EqualTo(SemanticSourceKind.Unresolved));
    }

    [Test]
    public void Analyze_WithMappedArgument_RetainsSuppliedArgumentContext()
    {
        const string text = "with(items := {{name := \"item\", suffix := \"wrong\"}}, suffix := \"!\", .items | map(.name | suffix(^.suffix)))";
        var source = Analyze(text).References.Last().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Expression));
        Assert.That(source.Syntax!.Text, Is.EqualTo(text));
        Assert.That(Expression.Create(text).Evaluate(null), Is.EqualTo(new[] { "item!" }));
    }

    [Test]
    public void Analyze_ShorthandMapping_UsesElementSource()
    {
        const string text = ".items |> (.name)";
        var source = Analyze(text).References.Last().Source;
        Assert.That(source.Kind, Is.EqualTo(SemanticSourceKind.Element));
        Assert.That(Slice(text, source.Span!.Value), Is.EqualTo(".items"));
    }

    [Test]
    public void Analyze_UnicodeBeforeReference_PreservesOffsets()
    {
        const string text = "\"é😀\" | suffix(.a.b)";
        Assert.That(Analyze(text).References.Select(reference => Slice(text, reference.Span)), Is.EqualTo(new[] { ".a", ".b" }));
    }

    [Test]
    public void Analyze_ReusedAnalyzer_DoesNotRetainPreviousResults()
    {
        var analyzer = new SemanticAnalyzer();
        var first = analyzer.Analyze(".one");
        var second = analyzer.Analyze(".two");
        Assert.That(first.References.Single().Syntax.Text, Is.EqualTo(".one"));
        Assert.That(second.References.Single().Syntax.Text, Is.EqualTo(".two"));
    }

    [Test]
    public void Analyze_RecordProjection_ReadsPipelineInput()
    {
        const string text = "{a := {b := 42}} | record(value := .a.b)";
        var references = Analyze(text).References;
        Assert.That(Slice(text, references[0].Source.Span!.Value), Is.EqualTo("{a := {b := 42}}"));
        Assert.That(Slice(text, references[1].Source.Span!.Value), Is.EqualTo(".a"));
    }

    [Test]
    public void Analyze_NestedClosedExpression_RootIsItsOwnSource()
    {
        const string text = "suffix(({name := \"inner\"} | ^.name))";
        var source = Analyze(text).References.Single().Source;
        Assert.That(Slice(text, source.Span!.Value), Is.EqualTo("{name := \"inner\"}"));
        Assert.That(Expression.Create(text).Evaluate("outer"), Is.EqualTo("outerinner"));
    }

    [Test]
    public void Analyze_ImplicitAccumulator_PreservesPipelineSupplier()
    {
        const string text = ".items | first | .name";
        Assert.That(Slice(text, Analyze(text).References.Last().Source.Span!.Value), Is.EqualTo("first"));
    }

    [Test]
    public void Analyze_ArraySourceWithClosedElement_RetainsSupplyingEvaluationRoot()
    {
        const string text = "{...({name := \"inner\"} | ^.names)} | first";
        var input = new Dictionary<string, object?> { ["names"] = new[] { "outer" } };
        Assert.That(Analyze(text).References.Single().Source.Kind, Is.EqualTo(SemanticSourceKind.ExternalInput));
        Assert.That(Expression.Create(text).Evaluate(input), Is.EqualTo("outer"));
    }

    private static SemanticAnalysis Analyze(string text)
    {
        var result = new SemanticAnalyzer().Analyze(text);
        Assert.That(result.Diagnostics, Is.Empty);
        return result;
    }

    private static string Slice(string text, SourceSpan span) => text.Substring(span.Start, span.Length);
}
