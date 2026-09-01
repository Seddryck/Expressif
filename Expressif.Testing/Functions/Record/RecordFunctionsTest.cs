using Expressif.Functions.Record;
using Expressif.Values;
using Expressif.Testing.Conformance;
using RecordFunction = Expressif.Functions.Record.Record;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Testing.Functions.Record;

public class RecordFunctionsTest
{
    [Conformance]
    public void Record_Valid_Spread(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Conformance]
    public void Field_Valid_Numeric(object? value, string expression, decimal expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Field_Valid_Null(object? value, string expression, object? expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void With_Valid_Text(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void With_Valid_Numeric(object? value, string expression, decimal expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void With_DuplicateProjectionName_ThrowsBindingDiagnostic()
        => Assert.That(
            () => Expression.Create("with(name := .firstName, name := .lastName, .name)"),
            Throws.TypeOf<BindingException>().With.Message.EqualTo("Duplicate projection 'name' in with(...)."));

    [TestCase("with(.name)")]
    [TestCase("with(name := .name)")]
    [TestCase("with(.name, .name)")]
    public void With_InvalidShape_ThrowsBindingDiagnostic(string source)
        => Assert.That(
            () => Expression.Create(source),
            Throws.TypeOf<BindingException>().With.Message.EqualTo(
                "Function 'with' expects one or more named projections followed by a body expression."));

    [Test]
    public void With_AggregatedProjections_ComposesTemporaryRecord()
    {
        var input = new Dictionary<string, object?>
        {
            ["orders"] = new object?[]
            {
                new Dictionary<string, object?> { ["active"] = true, ["amount"] = 12m },
                new Dictionary<string, object?> { ["active"] = false, ["amount"] = 8m },
                new Dictionary<string, object?> { ["active"] = true, ["amount"] = 10m },
            },
        };
        var expression = Expression.Create("""
            with(
                active-count := .orders | filter(.active) | count,
                total := .orders | map(.amount) | sum,
                record(
                    active-count := .active-count,
                    total := .total,
                    mean := .total | divide(.active-count)
                )
            )
            """);

        var result = (ValueRecord)expression.Evaluate(input)!;

        Assert.Multiple(() =>
        {
            Assert.That(result["active-count"], Is.EqualTo(2));
            Assert.That(result["total"], Is.EqualTo(30m));
            Assert.That(result["mean"], Is.EqualTo(15m));
        });
    }

    [Test]
    public void Field_Evaluate_DictionaryValue_ReturnsExpectedValue()
    {
        var function = new Field(() => "name");
        var input = new Dictionary<string, object?> { ["name"] = "Alice" };

        var result = function.Evaluate(input);

        Assert.That(result, Is.EqualTo("Alice"));
    }

    [Test]
    public void Field_Evaluate_RecordValue_ReturnsExpectedValue()
    {
        var function = new Field(() => "value");
        var input = new ValueRecord();
        input.Set("value", 2);

        var result = function.Evaluate(input);

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Field_Evaluate_MissingField_ReturnsNull()
    {
        var function = new Field(() => "missing");
        var input = new Dictionary<string, object?> { ["name"] = "Alice" };

        Assert.That(function.Evaluate(input), Is.Null);
    }

    [Test]
    public void Record_Evaluate_NoEntries_ReturnsEmptyRecord()
    {
        var function = new RecordFunction();
        var result = (ValueRecord)function.Evaluate("ignored")!;

        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public void Record_DeclaresValueSpreadAwareness()
        => Assert.That(new RecordFunction(), Is.InstanceOf<Expressif.Functions.IValueSpreadAware>());

    [Test]
    public void Record_Evaluate_NamedEntries_PreservesDeclarationOrder()
    {
        var function = new RecordFunction(() => new[]
        {
            RecordEntryEvaluator.Named("name", _ => "Alice"),
            RecordEntryEvaluator.Named("age", _ => 36),
            RecordEntryEvaluator.Named("active", _ => true),
        });

        var result = (ValueRecord)function.Evaluate(null)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "name", "age", "active" }));
            Assert.That(result["name"], Is.EqualTo("Alice"));
            Assert.That(result["age"], Is.EqualTo(36));
            Assert.That(result["active"], Is.EqualTo(true));
        });
    }

    [Test]
    public void Record_Evaluate_SpreadDictionary_ThenOverride_LastWins()
    {
        var input = new Dictionary<string, object?>
        {
            ["name"] = "Alice",
            ["country"] = "Belgium"
        };

        var function = new RecordFunction(() => new[]
        {
            RecordEntryEvaluator.Spread(value => value),
            RecordEntryEvaluator.Named("name", _ => "ALICE")
        });

        var result = (ValueRecord)function.Evaluate(input)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "name", "country" }));
            Assert.That(result["name"], Is.EqualTo("ALICE"));
            Assert.That(result["country"], Is.EqualTo("Belgium"));
        });
    }

    [Test]
    public void Record_Evaluate_SpreadScalar_ThrowsSpreadArgumentException()
    {
        var function = new RecordFunction(() => new[]
        {
            RecordEntryEvaluator.Spread(value => value)
        });

        Assert.That(
            () => function.Evaluate("Alice"),
            Throws.TypeOf<SpreadArgumentException>()
                .With.Message.EqualTo("Spread argument must evaluate to a record."));
    }

    [Test]
    public void Record_Evaluate_EntriesEvaluateAgainstSameInput()
    {
        var input = new Dictionary<string, object?> { ["name"] = "Alice" };

        var function = new RecordFunction(() => new[]
        {
            RecordEntryEvaluator.Named("name", x => new Field(() => "name").Evaluate(x)),
            RecordEntryEvaluator.Named("display", x => $"{new Field(() => "name").Evaluate(x)}!")
        });

        var result = (ValueRecord)function.Evaluate(input)!;

        Assert.Multiple(() =>
        {
            Assert.That(result["name"], Is.EqualTo("Alice"));
            Assert.That(result["display"], Is.EqualTo("Alice!"));
        });
    }
}
