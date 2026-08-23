using Expressif.Functions;
using Expressif.Bindings;
using Expressif.Values;
using Expressif.Values.Special;
using System.Data;
using System.Diagnostics;

namespace Expressif.Testing;

public class ExpressionTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Evaluate_SingleFunctionWithoutParameter_Valid()
    {
        var expression = Expression.Create("lower");
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola tesla"));
    }

    [Test]
    public void Evaluate_SingleFunctionWithOneParameter_Valid()
    {
        var expression = Expression.Create("remove-chars(`a`)");
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("Nikol Tesl"));
    }

    [Test]
    public void Evaluate_TwoFunctions_Valid()
    {
        var expression = Expression.Create("lower | remove-chars(\"a\")");
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikol tesl"));
    }

    [Test]
    public void Evaluate_VariableAsParameter_Valid()
    {
        var context = new Context();
        context.Variables.Add<char>("myChar", 'k');

        var expression = Expression.Create("lower | remove-chars(@myChar)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("niola tesla"));
    }

    [Test]
    public void Evaluate_VariableAsParameterDoublePass_Valid()
    {
        var context = new Context();
        var expression = Expression.Create("lower | remove-chars(@myChar)", context);

        context.Variables.Add<char>("myChar", 'k');
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("niola tesla"));
        context.Variables.Set("myChar", 'a');
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol tesl"));
    }

    [Test]
    public void Evaluate_ObjectPropertyAsParameter_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new { CharToBeRemoved = 't' });

        var expression = Expression.Create("lower | remove-chars(^.CharToBeRemoved)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola esla"));
    }

    [Test]
    public void Evaluate_ObjectPropertyAsParameterDoublePass_Valid()
    {
        var context = new Context();
        var expression = Expression.Create("lower | remove-chars(^.CharToBeRemoved)", context);

        context.CurrentObject.Set(new { CharToBeRemoved = 't' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola esla"));

        context.CurrentObject.Set(new { CharToBeRemoved = 'k' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("niola tesla"));
    }

    [Test]
    public void Evaluate_ObjectIndexAsParameter_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<char>() { 'e', 's' });

        var expression = Expression.Create("lower | remove-chars(^.1)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola tela"));
    }

    [Test]
    public void Evaluate_ObjectIndexAsParameterDoublePass_Valid()
    {
        var context = new Context();
        var expression = Expression.Create("lower | remove-chars(^.1)", context);

        context.CurrentObject.Set(new List<char>() { 'e', 's' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tela"));

        context.CurrentObject.Set(new List<char>() { 'e', 'o' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikla tesla"));
    }

    [Test]
    public void Evaluate_AliasesPrefix_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<char>() { 'e', 's' });

        var expression = Expression.Create("text-to-lower | text-to-remove-chars(^.1)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola tela"));
    }

    [Test]
    [SetCulture("en-US")]
    public void Evaluate_AliasesDateTime_Valid()
    {
        var expression = Expression.Create("dateTime-to-add(#\"04:00:00\", 4)", new Context());
        var result = expression.Evaluate(DateTime.Parse("2023-12-28 02:00:00"));
        Assert.That(result, Is.EqualTo(DateTime.Parse("2023-12-28 18:00:00")));
    }

    [Test]
    public void Evaluate_DurationBetween_Valid()
    {
        var expression = Expression.Create("duration-between(#\"2026-08-06T10:30:00\")", new Context());
        var result = expression.Evaluate(DateTime.Parse("2026-08-06T12:00:00"));
        Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(90)));
    }

    [Test]
    public void Evaluate_DurationBetweenIncompatiblePrevious_Null()
    {
        var expression = Expression.Create("duration-between(\"invalid\")", new Context());
        Assert.That(expression.Evaluate(DateTime.Parse("2026-08-06T12:00:00")), Is.Null);
    }

    [Test]
    public void Evaluate_CalendarCatholic_Valid()
    {
        var expression = Expression.Create("calendar-catholic(\"eAsTeR sUnDaY\")", new Context());
        var result = expression.Evaluate(2023);
        Assert.That(result, Is.EqualTo(DateTime.Parse("2023-04-09")));
    }

    [Test]
    public void Evaluate_CalendarCatholicWithKind_Valid()
    {
        var expression = Expression.Create("calendar-catholic(\"The Assumption\", \"Utc\")", new Context());
        var result = (DateTime)expression.Evaluate(2023)!;
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(DateTime.SpecifyKind(DateTime.Parse("2023-08-15"), DateTimeKind.Utc)));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    [TestCase("null-to-empty | count-chars")]
    [TestCase("null-to-empty | text-to-length")]
    [TestCase("null-to-empty | length")]
    public void Evaluate_AliasesAllStyles_Valid(string code)
    {
        var expression = Expression.Create(code, new Context());
        var result = expression.Evaluate("foo");
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Evaluate_FunctionAsParameter_Valid()
    {
        var context = new Context();
        context.Variables.Add<int>("myVar", 6);
        context.CurrentObject.Set(new List<decimal>() { 15, 8, 3 });

        var expression = Expression.Create("lower | skip-last-chars( {@myVar | subtract(^.2) })", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola te"));
    }

    [Test]
    public void Evaluate_FunctionWithIntegerForDecimal_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<int>() { 2, 3 });

        var expression = Expression.Create("numeric-to-multiply(^.1)", context);
        var result = expression.Evaluate(10);
        Assert.That(result, Is.EqualTo(30));
    }

    [Test]
    public void Evaluate_FunctionWithTextForDateTime_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<string>() { "2020-01-01", "2021-12-31" });

        var expression = Expression.Create("dateTime-to-clip(^.0, ^.1)", context);
        var result = expression.Evaluate("2018-01-01");
        Assert.That(result, Is.EqualTo(new DateTime(2020, 01, 01)));
    }

    [Test]
    public void Evaluate_ArrayLiteralPipeSum_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | sum");
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(6m));
    }

    [Test]
    public void Evaluate_VariableArrayPipeCount_Valid()
    {
        var context = new Context();
        context.Variables.Add<int[]>("arr", new[] { 1, 2, 3, 4 });

        var expression = new ClosedExpression("@arr | count", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_ObjectPropertyArrayPipeSum_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new { Values = new object[] { "1", 2, true } });

        var expression = new ClosedExpression("^.Values | sum", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(4m));
    }

    [Test]
    public void Evaluate_ObjectIndexArrayPipeMax_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<object> { new[] { 1, 9, 4 } });

        var expression = new ClosedExpression("^.0 | max", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(9m));
    }

    [Test]
    public void Evaluate_OpenExpression_StartWithArrayFunctionThenAdd_Valid()
    {
        var expression = Expression.Create("sum | add(4)");
        var result = expression.Evaluate(new[] { 1, 2, 3 });
        Assert.That(result, Is.EqualTo(10m));
    }

    [Test]
    public void Evaluate_DirectAccumulatorSyntax_StillScalar()
    {
        var expression = new ClosedExpression("{1,2,3} | sum");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(6m));
    }

    [TestCase("{#true,#true,#true} | every", true)]
    [TestCase("{#true,#false,#true} | every", false)]
    [TestCase("{#false,#true,#false} | any", true)]
    [TestCase("{#false,#false,#false} | any", false)]
    [TestCase("{} | every", true)]
    [TestCase("{} | any", false)]
    public void Evaluate_DirectBooleanAccumulatorSyntax_Valid(string code, bool expected)
        => Assert.That(new ClosedExpression(code).Evaluate(), Is.EqualTo(expected));

    [Test]
    public void Evaluate_ArrayPipeMapMultiply_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | map(multiply(2))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 2m, 4m, 6m }));
    }

    [Test]
    public void Evaluate_ArrayPipeMapAdd_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | map(add(10))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 11m, 12m, 13m }));
    }

    [Test]
    public void Evaluate_ArrayMapShorthandPipeline_Valid()
    {
        var expression = new ClosedExpression("{-1,2,-3} |> (absolute | add(5)) | reverse");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 8m, 7m, 6m }));
    }

    [TestCase("{1,12,5,42,17} |> add(1) | sum")]
    [TestCase("{1,12,5,42,17} |> (add(1)) | sum")]
    public void Evaluate_ArrayMapShorthand_ResumesParentPipeline(string code)
    {
        var result = new ClosedExpression(code).Evaluate();

        Assert.That(result, Is.EqualTo(82m));
    }

    [Test]
    public void Evaluate_LeadingMapPipeline_UsesImplicitInput()
    {
        var result = Expression.Create("|> add(1)").Evaluate(new object?[] { 10, 20 });

        Assert.That(result, Is.EqualTo(new object?[] { 11m, 21m }));
    }

    [TestCase("|> add(1) | sum")]
    [TestCase("|> (add(1)) | sum")]
    public void Evaluate_LeadingMapShorthand_ResumesParentPipeline(string code)
    {
        var result = Expression.Create(code).Evaluate(new object?[] { 1, 12, 5, 42, 17 });

        Assert.That(result, Is.EqualTo(82m));
    }

    [Test]
    public void Evaluate_ParenthesizedMapShorthand_ContainsInnerPipeline()
    {
        var result = Expression.Create("|> (absolute | add(1)) | sum")
            .Evaluate(new object?[] { -1, 12, -5, 42, -17 });

        Assert.That(result, Is.EqualTo(82m));
    }

    [Test]
    public void Evaluate_StringArrayPipeMapUpper_Valid()
    {
        var expression = new ClosedExpression("{\"alice\",\"bob\"} | map(upper)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { "ALICE", "BOB" }));
    }

    [Test]
    public void Evaluate_ArrayPipeMapPredicate_Valid()
    {
        var expression = new ClosedExpression("{10,15} | map(even)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { true, false }));
    }

    [Test]
    public void Evaluate_Map_PreservesCardinality()
    {
        var expression = new ClosedExpression("{1,2,3,4} | map(add(1))");
        var result = expression.Evaluate() as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Length.EqualTo(4));
    }

    [Test]
    public void Evaluate_EmptyArrayPipeMap_EmptyArray()
    {
        var expression = new ClosedExpression("{} | map(add(1))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(Array.Empty<object?>()));
    }

    [Test]
    public void Evaluate_ArrayPipeFilterGreaterThan_Valid()
    {
        var expression = new ClosedExpression("{1,2,3,4} | filter(greater-than(2))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 3m, 4m }));
    }

    [Test]
    public void Evaluate_ArrayPipeFilterEven_Valid()
    {
        var expression = new ClosedExpression("{1,2,3,4,5} | filter(even)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 2m, 4m }));
    }

    [Test]
    public void Evaluate_StringArrayPipeFilterStartsWith_Valid()
    {
        var expression = new ClosedExpression("{\"alice\",\"bob\",\"anna\"} | filter(starts-with(\"a\"))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { "alice", "anna" }));
    }

    [Test]
    public void Evaluate_Record_FieldAndCurrentObject_UseDifferentSources()
    {
        var context = new Context();
        context.CurrentObject.Set(new
        {
            name = "Cedric",
            customer = new Dictionary<string, object?>
            {
                ["name"] = "Alice",
                ["grz"] = "hello"
            }
        });

        var expression = new ClosedExpression("^.customer | record(customerName := field(name), requestedBy := ^.name)", context);
        var result = (RecordValue)expression.Evaluate()!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(["customerName", "requestedBy"]));
            Assert.That(result["customerName"], Is.EqualTo("Alice"));
            Assert.That(result["requestedBy"], Is.EqualTo("Cedric"));
        });
    }

    [Test]
    public void Evaluate_FieldShorthand_IsEquivalentToFieldFunction()
    {
        var input = new Dictionary<string, object?> { ["name"] = "Alice" };

        Assert.That(Expression.Create(".name").Evaluate(input),
            Is.EqualTo(Expression.Create("field(name)").Evaluate(input)));
    }

    [Test]
    public void Evaluate_FieldShorthand_NestedPipeline_ReadsNestedField()
    {
        var input = new Dictionary<string, object?>
        {
            ["address"] = new Dictionary<string, object?> { ["city"] = "Brussels" }
        };

        Assert.That(Expression.Create(".address | .city").Evaluate(input), Is.EqualTo("Brussels"));
    }

    [Test]
    public void Evaluate_FieldShorthand_MapAndFilter_UsesEachItemAsInput()
    {
        var context = new Context();
        context.CurrentObject.Set(new
        {
            customers = new object?[]
            {
                new Dictionary<string, object?> { ["name"] = "Alice", ["active"] = true },
                new Dictionary<string, object?> { ["name"] = "Bob", ["active"] = false }
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(new ClosedExpression("^.customers |> (.name)", context).Evaluate(),
                Is.EqualTo(new object?[] { "Alice", "Bob" }));
            Assert.That(new ClosedExpression("^.customers | filter(.active) |> (.name)", context).Evaluate(),
                Is.EqualTo(new object?[] { "Alice" }));
        });
    }

    [Test]
    public void Evaluate_FieldShorthand_RecordConstruction_UsesIncomingRecord()
    {
        var input = new Dictionary<string, object?> { ["name"] = "Alice" };
        var result = (RecordValue)Expression.Create("record(customer-name := .name)").Evaluate(input)!;

        Assert.That(result["customer-name"], Is.EqualTo("Alice"));
    }

    [Test]
    public void Evaluate_FieldShorthand_NullField_ReturnsNull()
    {
        var input = new Dictionary<string, object?> { ["name"] = null };

        Assert.That(Expression.Create(".name").Evaluate(input), Is.Null);
    }

    [TestCaseSource(nameof(FieldShorthandEdgeCases))]
    public void Evaluate_FieldShorthand_EdgeCasesMatchLongForm(object? input)
    {
        var longForm = Assert.Catch(() => Expression.Create("field(name)").Evaluate(input));
        var shorthand = Assert.Catch(() => Expression.Create(".name").Evaluate(input));

        Assert.That(shorthand, Is.TypeOf(longForm!.GetType()));
    }

    private static IEnumerable<TestCaseData> FieldShorthandEdgeCases()
    {
        yield return new TestCaseData(new Dictionary<string, object?>()).SetName("Missing field");
        yield return new TestCaseData("not a record").SetName("Non-record input");
        yield return new TestCaseData(null).SetName("Null input");
    }

    [Test]
    public void Evaluate_Record_WithSpreadAndOverride_PreservesOrderAndOverridesLast()
    {
        var input = new Dictionary<string, object?>
        {
            ["name"] = "Alice",
            ["country"] = "Belgium"
        };

        var expression = Expression.Create("record(..., name := field(name) | upper, age := 36)");
        var result = (RecordValue)expression.Evaluate(input)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "name", "country", "age" }));
            Assert.That(result["name"], Is.EqualTo("ALICE"));
            Assert.That(result["country"], Is.EqualTo("Belgium"));
            Assert.That(result["age"], Is.EqualTo(36));
        });
    }

    [Test]
    public void Evaluate_Record_SpreadNonRecord_GeneratesUniqueUnnamedField()
    {
        var expression = Expression.Create("record(__NONAME_0 := reserved, ...)");
        var result = (RecordValue)expression.Evaluate("Alice")!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "__NONAME_0", "__NONAME_1" }));
            Assert.That(result["__NONAME_0"], Is.EqualTo("reserved"));
            Assert.That(result["__NONAME_1"], Is.EqualTo("Alice"));
        });
    }

    [Test]
    public void Evaluate_Record_DuplicateExplicitField_ThrowsParseException()
    {
        Assert.That(
            () => Expression.Create("record(name := field(name), name := field(preferred-name))"),
            Throws.TypeOf<BindingException>());
    }

    [Test]
    public void Evaluate_Record_EmbeddingIncomingValue_DoesNotExpand()
    {
        var expression = Expression.Create("record(original := ..., normalized := upper)");
        var result = (RecordValue)expression.Evaluate("Alice")!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "original", "normalized" }));
            Assert.That(result["original"], Is.EqualTo("Alice"));
            Assert.That(result["normalized"], Is.EqualTo("ALICE"));
        });
    }

    [Test]
    public void Evaluate_RecordLiteral_ParsesTypedValues()
    {
        var expression = new ClosedExpression("{name := \"Alice\", active := #true, retries := 3, ratio := 1.5, missing := #null}");
        var result = (RecordValue)expression.Evaluate()!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "name", "active", "retries", "ratio", "missing" }));
            Assert.That(result["name"], Is.EqualTo("Alice"));
            Assert.That(result["active"], Is.True);
            Assert.That(result["retries"], Is.EqualTo(3));
            Assert.That(result["ratio"], Is.EqualTo(1.5m));
            Assert.That(result["missing"], Is.Null);
        });
    }

    [Test]
    public void Evaluate_EmptyArrayPipeFilter_EmptyArray()
    {
        var expression = new ClosedExpression("{} | filter(even)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(Array.Empty<object?>()));
    }

    [Test]
    public void Evaluate_FilterWithNonPredicateExpression_Throws()
    {
        var expression = new ClosedExpression("{1,2,3} | filter(add(1))");

        Assert.That(() => expression.Evaluate(), Throws.TypeOf<NotImplementedFunctionException>());
    }

    [Test]
    public void Evaluate_EmptyArrayPipeAggregators_Valid()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new ClosedExpression("{} | count").Evaluate(), Is.Zero);
            Assert.That(new ClosedExpression("{} | sum").Evaluate(), Is.Zero);
            Assert.That(new ClosedExpression("{} | min").Evaluate(), Is.Null);
            Assert.That(new ClosedExpression("{} | max").Evaluate(), Is.Null);
            Assert.That(new ClosedExpression("{} | first").Evaluate(), Is.Null);
            Assert.That(new ClosedExpression("{} | last").Evaluate(), Is.Null);
        }
    }
}
