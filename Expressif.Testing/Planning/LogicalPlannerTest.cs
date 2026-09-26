using System.Reflection;
using Expressif.Bindings;
using Expressif.Functions.Catalog;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class LogicalPlannerTest
{
    private static IEnumerable<TestCaseData> ParameterShapes()
    {
        yield return Shape(new LiteralParameter(1), "literal");
        yield return Shape(new QuotedLiteralParameter("quoted"), "literal");
        yield return Shape(new CallableReferenceParameter("compare-numeric"), "callable-reference");
        yield return Shape(new SortCriterionParameter(
            new TupleProjectionParameter(0),
            Expressif.Types.TypeRegistry.Resolve("integer"),
            Ascending: true,
            NullsFirst: false), "sort-criterion");
        yield return Shape(new VariableParameter("value"), "variable");
        yield return Shape(new IncomingValueParameter(), "incoming");
        yield return Shape(new ObjectPropertyParameter("name"), "field");
        yield return Shape(new EnclosingObjectPropertyParameter("name"), "field");
        yield return Shape(new ObjectIndexParameter(2), "field");
        yield return Shape(new TupleProjectionParameter(2, FromEnd: true), "tuple-at");
        yield return new TestCaseData(new TupleProjectionParameter(2), "tuple-at")
            .SetName("Value_TupleProjectionParameter_FromStart");
        yield return Shape(new ScopedTupleProjectionParameter(2, 3), "tuple-at");
        yield return Shape(new ArrayParameter([new LiteralParameter(1)]), "array");
        yield return Shape(new TupleParameter([new LiteralParameter(1)]), "tuple");
        yield return Shape(new VectorParameter([new TupleElementParameter(new LiteralParameter(1), IsSpread: true)]), "vector");
        yield return Shape(new PairParameter(new LiteralParameter("key"), new LiteralParameter(1)), "pair");
        yield return Shape(new GroupingParameter([new PairParameter(new LiteralParameter("key"), new LiteralParameter(1))]), "grouping");
        yield return Shape(new DictionaryParameter([new PairParameter(new LiteralParameter("key"), new LiteralParameter(1))]), "dictionary");
        yield return Shape(new RecordLiteralParameter([new RecordLiteralField("name", new LiteralParameter("value"))]), "record");
        yield return Shape(new RecordDefinitionParameter([
            new RecordNamedEntry("name", new LiteralParameter("value")),
            new RecordSpreadEntry(new IncomingValueParameter()),
        ]), "record");
        yield return Shape(new LetDefinitionParameter([
            new LetBinding("value", new LiteralParameter(10)),
        ]), "let-definition");
        yield return Shape(new OpenExpressionParameter(new OpenExpression([new Function("trim", [])])), "pipeline");
        yield return Shape(new InputExpressionParameter(new ClosedExpression(new LiteralParameter("text"), [new Function("trim", [])])), "pipeline");
        yield return Shape(new IntervalParameter(new IntervalBinding(
            new IntervalBoundBinding(IntervalBoundBindingKind.NegativeInfinity),
            new IntervalBoundBinding(IntervalBoundBindingKind.PositiveInfinity),
            false,
            false)), "interval");
        yield return Shape(new PositionalCoercionParameter(typeof(string)), "coercion");
        yield return Shape(new FieldCoercionParameter("name", typeof(string)), "field-coercion");
        yield return Shape(new TupleCoercionParameter(1, typeof(string)), "tuple-coercion");
        yield return Shape(new PredicationParameter(new SinglePredication(new Function("even", []))), "even");
        yield return Shape(new PredicationParameter(new PipelinePredication(new OpenExpression([new Function("even", [])]))), "pipeline");
        yield return Shape(new ControlFlowBranchParameter(new LiteralParameter(1), null), "branch");
        yield return new TestCaseData(
            new ControlFlowBranchParameter(new LiteralParameter(1), new LiteralParameter(true)),
            "branch").SetName("Value_ControlFlowBranchParameter_WithPredicate");
        yield return Shape(new WithDefinitionParameter(
            [new WithProjection("name", new LiteralParameter("value"))],
            new VariableParameter("name")), "with");
    }

    [Test]
    public void Plan_AliasAndCanonicalSyntax_ProduceEquivalentCalls()
    {
        var canonical = LogicalPlanner.Plan(ExpressionParser.Parse("upper"));
        var alias = LogicalPlanner.Plan(ExpressionParser.Parse("text-to-upper"));

        Assert.Multiple(() =>
        {
            Assert.That(((LogicalCall)alias.Pipeline.Items.Single()).Function,
                Is.EqualTo(((LogicalCall)canonical.Pipeline.Items.Single()).Function));
            Assert.That(((LogicalCall)alias.Pipeline.Items.Single()).Arguments, Is.Empty);
        });
    }

    [Test]
    public void Plan_StructuralOperator_CopiesMachineReadableSemanticsWithoutCatalogProse()
    {
        var call = SingleCall("map(upper)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Semantics,
                Is.EqualTo(new PlannerSemanticsDescriptor("preserved", "per-element", "preserved")));
            Assert.That(call.Function.Traversal,
                Is.EqualTo(new PlannerTraversalDescriptor("incoming", "array-element")));
            Assert.That(call.Arguments.Single().Parameter.Evaluation,
                Is.EqualTo(new PlannerEvaluationDescriptor("per-element", Context: "traversal")));
        });
    }

    [Test]
    public void Plan_NamedArguments_AreOrderedByCanonicalParameters()
    {
        var call = SingleCall("add(times := 2, value := 5)");

        Assert.That(call.Arguments.Select(argument => argument.Parameter.Name),
            Is.EqualTo(new[] { "value", "times" }));
        Assert.That(call.Arguments.Select(argument => ((LogicalLiteral)argument.Value!).Value),
            Is.EqualTo(new object[] { 5m, 2m }));
    }

    [Test]
    public void Plan_OmittedArgument_PreservesCatalogOmissionInsteadOfMaterializingDefault()
    {
        var call = SingleCall("add(5)");
        var omitted = call.Arguments.Single(argument => argument.Parameter.Name == "times");

        Assert.Multiple(() =>
        {
            Assert.That(omitted.IsExplicit, Is.False);
            Assert.That(omitted.Value, Is.Null);
            Assert.That(omitted.Omission?.Mode, Is.EqualTo(global::Expressif.Functions.Catalog.ParameterOmissionMode.Constant));
            Assert.That(omitted.Omission?.Value.GetDecimal(), Is.EqualTo(1m));
        });
    }

    [Test]
    public void Plan_SpreadArgument_PreservesSpreadOnCanonicalParameter()
    {
        var call = SingleCall("array(...{1, 2})");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo("array"));
            Assert.That(call.Arguments.Single().Parameter.Name, Is.EqualTo("values"));
            Assert.That(call.Arguments.Single().IsSpread, Is.True);
        });
    }

    [TestCase(".age", "field", 0)]
    [TestCase("^.age", "field", 1)]
    [TestCase("$1", "tuple-at", 0)]
    public void Plan_ReferenceSyntax_UsesOrdinaryCatalogCalls(string source, string name, int contextDepth)
    {
        var call = SingleCall(source);

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo(name));
            Assert.That(call.ContextDepth, Is.EqualTo(contextDepth));
            if (name == "tuple-at")
                Assert.That(((LogicalLiteral)call.Arguments.Single().Value!).Type, Is.EqualTo("integer"));
        });
    }

    [TestCase("{1, 2, 3}", "array")]
    [TestCase("T(1, 2)", "tuple")]
    public void Plan_ValueConstructorSyntax_UsesOrdinaryCalls(string source, string name)
    {
        var plan = LogicalPlanner.Plan(ExpressionParser.Parse(source));

        Assert.That(((LogicalCall)plan.Pipeline.Items.Single()).Function.Name, Is.EqualTo(name));
    }

    [Test]
    public void Plan_DoesNotApplyRuntimeCoercionInsertion()
    {
        var plan = LogicalPlanner.Plan(ExpressionParser.Parse("split(\",\") | length"));

        Assert.That(plan.Pipeline.Items.OfType<LogicalCall>().Select(call => call.Function.Name),
            Is.EqualTo(new[] { "split", "length" }));
    }

    [TestCase("#all", "all", "#all")]
    [TestCase("#less", "ordering", "#less")]
    [TestCase("#equal", "ordering", "#equal")]
    [TestCase("#greater", "ordering", "#greater")]
    public void Plan_SpecialScalarLiteral_UsesPortableLogicalRepresentation(
        string source,
        string expectedType,
        string expectedValue)
    {
        var literal = (LogicalLiteral)LogicalPlanner.Plan(ExpressionParser.Parse(source)).Pipeline.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(literal.Type, Is.EqualTo(expectedType));
            Assert.That(literal.Value, Is.EqualTo(expectedValue));
        });
    }

    [Test]
    public void Plan_LetDefinition_PreservesBindingNameAndValue()
    {
        var call = SingleCall("let(value := 10)");
        var definition = (LogicalCall)call.Arguments.Single().Value!;
        var binding = definition.Arguments.Single();

        Assert.Multiple(() =>
        {
            Assert.That(definition.Function.Name, Is.EqualTo("let-definition"));
            Assert.That(binding.Parameter.Name, Is.EqualTo("value"));
            Assert.That(((LogicalLiteral)binding.Value!).Value, Is.EqualTo(10m));
        });
    }

    [Test]
    public void Plan_SortCriterion_PreservesSelectorTypeDirectionAndNullPlacement()
    {
        var call = SingleCall("sort-by($0 -> :integer)");
        var criterion = (LogicalCall)call.Arguments.Single().Value!;

        Assert.Multiple(() =>
        {
            Assert.That(criterion.Function.Name, Is.EqualTo("sort-criterion"));
            Assert.That(((LogicalCall)criterion.Arguments[0].Value!).Function.Name, Is.EqualTo("tuple-at"));
            Assert.That(criterion.Arguments[1].Value,
                Is.EqualTo(new LogicalLiteral("type", "integer")));
            Assert.That(criterion.Arguments[2].Value,
                Is.EqualTo(new LogicalLiteral("boolean", true)));
            Assert.That(criterion.Arguments[3].Value,
                Is.EqualTo(new LogicalLiteral("boolean", false)));
        });
    }

    [Test]
    public void Plan_CallableReference_PreservesReferencedName()
    {
        var call = SingleCall("sort-term(1, compare-numeric~)");
        var reference = (LogicalCall)call.Arguments.Single(argument => argument.Parameter.Name == "comparer").Value!;

        Assert.Multiple(() =>
        {
            Assert.That(reference.Function.Name, Is.EqualTo("callable-reference"));
            Assert.That(reference.Arguments.Single().Parameter.Name, Is.EqualTo("name"));
            Assert.That(reference.Arguments.Single().Value,
                Is.EqualTo(new LogicalLiteral("text", "compare-numeric")));
        });
    }

    [TestCaseSource(nameof(ParameterShapes))]
    public void Value_EverySupportedParameterShape_ProducesLogicalValue(IParameter parameter, string expectedShape)
    {
        var value = InvokeValue(parameter);

        Assert.That(ShapeOf(value), Is.EqualTo(expectedShape));
    }

    [Test]
    public void Value_UnsupportedParameter_ThrowsPlanningException()
    {
        var exception = Assert.Throws<TargetInvocationException>(() => InvokeValue(new UnsupportedParameter()));

        Assert.That(exception!.InnerException, Is.TypeOf<LogicalPlanningException>()
            .And.Message.EqualTo("Unsupported parameter 'UnsupportedParameter'."));
    }

    [Test]
    public void Value_RecordDefinitionWithUnsupportedEntry_ThrowsPlanningException()
    {
        var parameter = new RecordDefinitionParameter([new UnsupportedRecordEntry()]);

        var exception = Assert.Throws<TargetInvocationException>(() => InvokeValue(parameter));

        Assert.That(exception!.InnerException, Is.TypeOf<LogicalPlanningException>()
            .And.Message.EqualTo("Unsupported record entry 'UnsupportedRecordEntry'."));
    }

    [Test]
    public void NormalizeArguments_ParameterlessFunctionWithArgument_ThrowsPlanningException()
    {
        var exception = Assert.Throws<TargetInvocationException>(() => InvokeNormalizeArguments(
            "trim",
            [],
            [new FunctionArgument(null, new LiteralParameter("value"))]));

        Assert.That(exception!.InnerException, Is.TypeOf<LogicalPlanningException>()
            .And.Message.EqualTo("Function 'trim' does not accept arguments."));
    }

    [Test]
    public void NormalizeArguments_ParameterlessFunctionWithoutArgument_ReturnsEmptyList()
    {
        var arguments = InvokeNormalizeArguments("trim", [], []);

        Assert.That(arguments, Is.Empty);
    }

    private static LogicalCall SingleCall(string source)
        => (LogicalCall)LogicalPlanner.Plan(ExpressionParser.Parse(source)).Pipeline.Items.Single();

    private static TestCaseData Shape(IParameter parameter, string expected)
        => new TestCaseData(parameter, expected).SetName($"Value_{parameter.GetType().Name}");

    private static LogicalValue InvokeValue(IParameter parameter)
        => (LogicalValue)typeof(LogicalPlanner)
            .GetMethod("Value", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(new LogicalPlanner(), [parameter])!;

    private static IReadOnlyList<LogicalArgument> InvokeNormalizeArguments(
        string name,
        IReadOnlyList<FunctionParameterDocumentation> parameters,
        IReadOnlyList<FunctionArgument> supplied)
        => (IReadOnlyList<LogicalArgument>)typeof(LogicalPlanner)
            .GetMethod("NormalizeArguments", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(new LogicalPlanner(), [name, parameters, supplied])!;

    private static string ShapeOf(LogicalValue value) => value switch
    {
        LogicalLiteral => "literal",
        LogicalPipeline => "pipeline",
        LogicalCall call => call.Function.Name,
        _ => value.GetType().Name,
    };

    private sealed record UnsupportedParameter : IParameter;
    private sealed record UnsupportedRecordEntry : IRecordDefinitionEntry;
}
