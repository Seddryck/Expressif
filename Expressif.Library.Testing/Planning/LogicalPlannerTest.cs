using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class LogicalPlannerTest
{
    [Test]
    public void Plan_AliasAndCanonicalSyntax_ProduceEquivalentCalls()
    {
        var canonical = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse("upper"));
        var alias = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse("text-to-upper"));

        Assert.Multiple(() =>
        {
            Assert.That(((LogicalCall)alias.Pipeline.Items.Single()).Function,
                Is.EqualTo(((LogicalCall)canonical.Pipeline.Items.Single()).Function));
            Assert.That(((LogicalCall)alias.Pipeline.Items.Single()).Arguments, Is.Empty);
        });
    }

    [Test]
    public void Plan_PredicateAlias_UsesCanonicalCatalogMetadata()
    {
        var call = SingleCall("greater-than(1)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo("is-greater-than"));
            Assert.That(call.Function.Kind, Is.EqualTo("predicate"));
            Assert.That(call.Function.Input, Is.EqualTo("numeric"));
            Assert.That(call.Function.Output, Is.EqualTo("boolean"));
            Assert.That(call.Arguments.Single().Parameter.Name, Is.EqualTo("reference"));
            Assert.That(call.Arguments.Single().Parameter.Type, Is.EqualTo("numeric"));
        });
    }

    [TestCase("first", "first-elements")]
    [TestCase("last", "last-elements")]
    public void Plan_CrossKindAliasWithArgument_ResolvesFunction(
        string alias,
        string canonical)
    {
        var call = SingleCall($"{alias}(1)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo(canonical));
            Assert.That(call.Function.Kind, Is.EqualTo("function"));
            Assert.That(call.Arguments.Single().Parameter.Name, Is.EqualTo("count"));
        });
    }

    [TestCase("first")]
    [TestCase("last")]
    public void Plan_AccumulatorArgument_ResolvesAccumulatorKind(string accumulator)
    {
        var fold = SingleCall($"fold({accumulator})");
        var pipeline = (LogicalPipeline)fold.Arguments.Single().Value!;
        var nested = (LogicalCall)pipeline.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(fold.Function.Kind, Is.EqualTo("function"));
            Assert.That(fold.Arguments.Single().Parameter.Name, Is.EqualTo("accumulator"));
            Assert.That(nested.Function.Name, Is.EqualTo(accumulator));
            Assert.That(nested.Function.Kind, Is.EqualTo("accumulator"));
            Assert.That(nested.Arguments, Is.Empty);
        });
    }

    [Test]
    public void Plan_UnknownOperator_UsesExplicitExtensionKind()
    {
        var call = SingleCall("custom-operator(1)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo("custom-operator"));
            Assert.That(call.Function.Kind, Is.EqualTo("extension"));
            Assert.That(call.Arguments.Single().Parameter.Name, Is.EqualTo("argument-0"));
        });
    }

    [Test]
    public void Plan_FilterPredicate_ContainsNoSyntheticArgumentMetadata()
    {
        var filter = SingleCall("filter(greater-than(1))");
        var predicatePipeline = (LogicalPipeline)filter.Arguments.Single().Value!;
        var predicate = (LogicalCall)predicatePipeline.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(predicate.Function.Name, Is.EqualTo("is-greater-than"));
            Assert.That(predicate.Arguments.Select(argument => argument.Parameter.Name),
                Is.EqualTo(new[] { "reference" }));
            Assert.That(predicate.Arguments.Select(argument => argument.Parameter.Type),
                Is.EqualTo(new[] { "numeric" }));
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
    public void Plan_SchemaAwareOperator_CopiesSchemaContract()
    {
        var call = SingleCall("map(upper)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Schema?.Input, Is.EqualTo("array<T>"));
            Assert.That(call.Function.Schema?.Output, Is.EqualTo("array<U>"));
            Assert.That(call.Function.Schema?.Parameters?["transformation"],
                Is.EqualTo(new PlannerParameterSchemaDescriptor("T", "U")));
            Assert.That(call.Function.Schema?.Classification, Is.EqualTo("contract"));
            Assert.That(call.Function.Schema?.NullableWhen, Is.EqualTo(new[] { "input" }));
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

    [TestCase("put")]
    [TestCase("put-present")]
    [TestCase("put-absent")]
    public void Plan_PutAssignments_UseCanonicalParameterAndPreserveEntryNames(string function)
    {
        var call = SingleCall($"{function}(first := 1, second := 2)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Arguments.Select(argument => argument.Parameter.Name),
                Is.EqualTo(new[] { "assignments", "assignments" }));
            Assert.That(call.Arguments.Select(EntryName), Is.EqualTo(new[] { "first", "second" }));
            Assert.That(call.Arguments.Select(argument => ((LogicalLiteral)EntryValue(argument)).Value),
                Is.EqualTo(new object[] { 1m, 2m }));
        });
    }

    [Test]
    public void Plan_TransformAs_PreservesOperationAndNamedExpressions()
    {
        var call = SingleCall("transform-as(trim, first := .first-name, second := .last-name)");

        Assert.Multiple(() =>
        {
            Assert.That(call.Arguments.Select(argument => argument.Parameter.Name),
                Is.EqualTo(new[] { "operation", "expressions", "expressions" }));
            Assert.That(((LogicalPipeline)call.Arguments[0].Value!).Items.Single(),
                Is.InstanceOf<LogicalCall>().And.Property(nameof(LogicalCall.Function))
                    .Property(nameof(PlannerFunctionDescriptor.Name)).EqualTo("trim"));
            Assert.That(call.Arguments.Skip(1).Select(EntryName), Is.EqualTo(new[] { "first", "second" }));
            Assert.That(call.Arguments.Skip(1).Select(argument =>
                    ((LogicalCall)((LogicalPipeline)EntryValue(argument)).Items.Single()).Function.Name),
                Is.EqualTo(new[] { "field", "field" }));
        });
    }

    [Test]
    public void Plan_With_PreservesNamedProjectionsAndSeparateBody()
    {
        var call = SingleCall("with(first := 1, second := 2, add(.first, .second))");

        Assert.Multiple(() =>
        {
            Assert.That(call.Arguments.Select(argument => argument.Parameter.Name),
                Is.EqualTo(new[] { "projections", "projections", "body" }));
            Assert.That(call.Arguments.Take(2).Select(EntryName), Is.EqualTo(new[] { "first", "second" }));
            Assert.That(((LogicalPipeline)call.Arguments[2].Value!).Items.Single(),
                Is.InstanceOf<LogicalCall>().And.Property(nameof(LogicalCall.Function))
                    .Property(nameof(PlannerFunctionDescriptor.Name)).EqualTo("add"));
        });
    }

    [Test]
    public void Plan_DuplicateEntry_ThrowsPlanningDiagnostic()
        => Assert.That(
            () => LogicalPlannerFactory.Create().Build(ExpressionParser.Parse("put(age := 1, age := 2)")),
            Throws.TypeOf<LogicalPlanningException>());

    [Test]
    public void Plan_OmittedArgument_PreservesCatalogOmissionInsteadOfMaterializingDefault()
    {
        var call = SingleCall("add(5)");
        var omitted = call.Arguments.Single(argument => argument.Parameter.Name == "times");

        Assert.Multiple(() =>
        {
            Assert.That(omitted.IsExplicit, Is.False);
            Assert.That(omitted.Value, Is.Null);
            Assert.That(omitted.Omission?.Mode, Is.EqualTo(PlannerOmissionMode.Constant));
            Assert.That(omitted.Omission?.Value.GetDecimal(), Is.EqualTo(1m));
        });
    }

    [TestCase("4 | power(2)", "exponent", 1)]
    [TestCase("16 | nth-root(2)", "exponent", 1)]
    [TestCase("10 | subtract(5)", "value,times", 1)]
    [TestCase("T(\"a\", \"b\", \"c\", \"d\") | swap", "first,second", 0)]
    [TestCase("#\"2024-01-15 12:30:45\" | change-of-hour(8)", "hour", 1)]
    [TestCase("#\"2024-01-15 12:30:45\" | change-of-minute(15)", "minute", 1)]
    [TestCase("#\"2024-01-15 12:30:45\" | change-of-second(10)", "second", 1)]
    [TestCase("#\"2024-01-15 12:30:45\" | change-of-month(6)", "month", 1)]
    [TestCase("#\"2024-01-15 12:30:45\" | change-of-year(2025)", "year", 1)]
    [TestCase("#\"2024-01-15 12:30:00\" | local-to-utc(\"UTC\")", "timeZoneLabel", 1)]
    [TestCase("#\"2024-01-15 12:30:00\" | utc-to-local(\"UTC\")", "timeZoneLabel", 1)]
    [TestCase("2024 | catholic-calendar(\"Easter Sunday\")", "event,kind", 1)]
    public void Plan_CorrectedCatalogExample_UsesCanonicalParameters(
        string source,
        string parameterNames,
        int explicitArguments)
    {
        var plan = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source));
        var call = plan.Pipeline.Items.OfType<LogicalCall>().Last();

        Assert.Multiple(() =>
        {
            Assert.That(call.Arguments.Select(argument => argument.Parameter.Name),
                Is.EqualTo(parameterNames.Split(',')));
            Assert.That(call.Arguments.Count(argument => argument.IsExplicit), Is.EqualTo(explicitArguments));
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
            if (name == "field")
                Assert.That(call.Function.Schema?.Intrinsic, Is.EqualTo("field"));
            if (name == "tuple-at")
                Assert.That(((LogicalLiteral)call.Arguments.Single().Value!).Type, Is.EqualTo("integer"));
        });
    }

    [TestCase("{1, 2, 3}", "array")]
    [TestCase("T(1, 2)", "tuple")]
    public void Plan_ValueConstructorSyntax_UsesOrdinaryCalls(string source, string name)
    {
        var plan = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source));

        Assert.That(((LogicalCall)plan.Pipeline.Items.Single()).Function.Name, Is.EqualTo(name));
    }

    [Test]
    public void Plan_DoesNotApplyRuntimeCoercionInsertion()
    {
        var plan = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse("split(\",\") | length"));

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
        var literal = (LogicalLiteral)LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source)).Pipeline.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(literal.Type, Is.EqualTo(expectedType));
            Assert.That(literal.Value, Is.EqualTo(expectedValue));
        });
    }

    [TestCase("apply(@_ | input :> @input)", false, new[] { "input" }, "variable")]
    [TestCase("apply(@_ | :> .first | upper)", false, new string[0], "field")]
    [TestCase("apply((left, right) :> @left)", true, new[] { "left", "right" }, "variable")]
    public void Plan_InputBinding_PreservesDeclarationAndBody(
        string source,
        bool positional,
        string[] names,
        string expectedBodyShape)
    {
        var apply = SingleCall(source);
        var expression = (LogicalPipeline)apply.Arguments.Single().Value!;
        var binding = (LogicalCall)expression.Items.Single();
        var plannedNames = (LogicalCall)binding.Arguments.Single(argument => argument.Parameter.Name == "names").Value!;
        var body = (LogicalPipeline)binding.Arguments.Single(argument => argument.Parameter.Name == "body").Value!;

        Assert.Multiple(() =>
        {
            Assert.That(binding.Function.Name, Is.EqualTo("input-binding"));
            Assert.That(binding.Arguments.Single(argument => argument.Parameter.Name == "positional").Value,
                Is.EqualTo(new LogicalLiteral("boolean", positional)));
            Assert.That(plannedNames.Arguments.Where(argument => argument.IsExplicit)
                .Select(argument => ((LogicalLiteral)argument.Value!).Value), Is.EqualTo(names));
            Assert.That(ShapeOf(body.Items.First()), Is.EqualTo(expectedBodyShape));
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
            Assert.That((criterion.Function.Schema?.Classification, criterion.Function.Schema?.Intrinsic),
                Is.EqualTo(("intrinsic", "sort-criterion")));
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

    [Test]
    public void Plan_NestedInputBindings_PreserveBothDeclarations()
    {
        var apply = SingleCall("apply(@_ | outer :> apply(@_ | inner :> @outer))");
        var outer = InputBindingFrom((LogicalPipeline)apply.Arguments.Single().Value!);
        var outerBody = (LogicalPipeline)outer.Arguments.Single(argument => argument.Parameter.Name == "body").Value!;
        var nestedApply = (LogicalCall)outerBody.Items.Single();
        var inner = InputBindingFrom((LogicalPipeline)nestedApply.Arguments.Single().Value!);

        Assert.Multiple(() =>
        {
            Assert.That(InputBindingNames(outer), Is.EqualTo(new[] { "outer" }));
            Assert.That(InputBindingNames(inner), Is.EqualTo(new[] { "inner" }));
            Assert.That(((LogicalPipeline)inner.Arguments.Single(argument => argument.Parameter.Name == "body").Value!)
                .Items.Single(), Is.InstanceOf<LogicalCall>()
                    .And.Property(nameof(LogicalCall.Function)).Property(nameof(PlannerFunctionDescriptor.Name))
                    .EqualTo("variable"));
        });
    }

    private static LogicalCall SingleCall(string source)
        => (LogicalCall)LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source)).Pipeline.Items.Single();

    private static LogicalCall InputBindingFrom(LogicalPipeline pipeline)
        => (LogicalCall)pipeline.Items.Single();

    private static string EntryName(LogicalArgument argument)
        => (string)((LogicalLiteral)((LogicalCall)argument.Value!).Arguments
            .Single(entryArgument => entryArgument.Parameter.Name == "name").Value!).Value!;

    private static LogicalValue EntryValue(LogicalArgument argument)
        => ((LogicalCall)argument.Value!).Arguments
            .Single(entryArgument => entryArgument.Parameter.Name == "value").Value!;

    private static object?[] InputBindingNames(LogicalCall binding)
        => ((LogicalCall)binding.Arguments.Single(argument => argument.Parameter.Name == "names").Value!)
            .Arguments.Where(argument => argument.IsExplicit)
            .Select(argument => ((LogicalLiteral)argument.Value!).Value)
            .ToArray();

    private static string ShapeOf(LogicalValue value) => value switch
    {
        LogicalLiteral => "literal",
        LogicalPipeline => "pipeline",
        LogicalCall call => call.Function.Name,
        _ => value.GetType().Name,
    };
}
