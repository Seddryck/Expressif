using Expressif.Functions.Record;
using Expressif.Values;
using ValueRecord = Expressif.Values.RecordValue;
using RecordFunction = Expressif.Functions.Record.Record;

namespace Expressif.Testing.Functions.Record;

public class RecordFunctionsTest
{
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
    public void Field_Evaluate_MissingField_ThrowsArgumentOutOfRangeException()
    {
        var function = new Field(() => "missing");
        var input = new Dictionary<string, object?> { ["name"] = "Alice" };

        Assert.That(() => function.Evaluate(input), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Record_Evaluate_NoEntries_ReturnsEmptyRecord()
    {
        var function = new RecordFunction();
        var result = (ValueRecord)function.Evaluate("ignored")!;

        Assert.That(result.Count, Is.EqualTo(0));
    }

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
            RecordEntryEvaluator.Spread(),
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
    public void Record_Evaluate_SpreadScalar_GeneratesUnnamedField()
    {
        var function = new RecordFunction(() => new[]
        {
            RecordEntryEvaluator.Spread()
        });

        var result = (ValueRecord)function.Evaluate("Alice")!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "__NONAME_0" }));
            Assert.That(result["__NONAME_0"], Is.EqualTo("Alice"));
        });
    }

    [Test]
    public void Record_Evaluate_SpreadScalar_AvoidsUnnamedCollision()
    {
        var function = new RecordFunction(() => new[]
        {
            RecordEntryEvaluator.Named("__NONAME_0", _ => "reserved"),
            RecordEntryEvaluator.Spread()
        });

        var result = (ValueRecord)function.Evaluate("Alice")!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Keys.ToArray(), Is.EqualTo(new[] { "__NONAME_0", "__NONAME_1" }));
            Assert.That(result["__NONAME_0"], Is.EqualTo("reserved"));
            Assert.That(result["__NONAME_1"], Is.EqualTo("Alice"));
        });
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
