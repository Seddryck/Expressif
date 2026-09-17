using System.Collections;
using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Record;

public class PairsTest
{
    [Conformance]
    public void Pairs_Valid_Array(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void FromPairs_Valid_Record(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("pairs")]
    [TestCase("from-pairs")]
    public void Operators_RejectNonCollectionInput(string expression)
    {
        foreach (var input in new object?[] { null, 42m, true, "text", new PairValue("a", 1), new TupleValue("a", 1) })
            Assert.That(() => Expression.Create(expression).Evaluate(input), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Pairs_RejectsArray()
        => Assert.That(() => ((IFunction)new Pairs()).Evaluate(new object?[] { 1, 2 }), Throws.TypeOf<ArgumentException>());

    [Test]
    public void FromPairs_RejectsRecord()
        => Assert.That(() => new FromPairs().Evaluate(new RecordValue()), Throws.TypeOf<ArgumentException>());

    [TestCase("{}")]
    [TestCase("T(\"a\", 1)")]
    [TestCase("{key := \"a\", value := 1}")]
    [TestCase("#null")]
    [TestCase("1")]
    public void FromPairs_RejectsNonPairElement(string element)
        => Assert.That(
            () => Expression.Create($"{{(\"valid\" => 1), {element}}} | from-pairs").Evaluate(null),
            Throws.TypeOf<ArgumentException>().With.Message.StartsWith("Every element supplied to from-pairs must be a pair."));

    [TestCase("{(#null => 1)}", "Every pair key must be coercible to text.")]
    [TestCase("{(\"a\" => 1), (\"a\" => 2)}", "Duplicate record field name 'a' after key coercion.")]
    [TestCase("{(2025 => 1), (\"2025\" => 2)}", "Duplicate record field name '2025' after key coercion.")]
    public void FromPairs_RejectsInvalidOrDuplicateKey(string input, string message)
        => Assert.That(
            () => Expression.Create($"{input} | from-pairs").Evaluate(null),
            Throws.TypeOf<ArgumentException>().With.Message.StartsWith(message));

    [Test]
    public void FromPairs_RejectsUncoercibleKey()
        => Assert.That(
            () => new FromPairs().Evaluate(new[] { new PairValue(new UncoercibleKey(), 1) }),
            Throws.TypeOf<ArgumentException>().With.Message.StartsWith("Every pair key must be coercible to text."));

    [Test]
    public void TypedRoundTrip_PreservesOrderNullPresenceAndValueIdentity()
    {
        var nested = new object?[] { 1m, new RecordValue() };
        var input = new RecordValue();
        input.Set("_id", nested);
        input.Set("missing", null);
        input.Set("name", "Alice");
        IFunction<RecordValue, PairValue[]> pairs = new Pairs();
        IFunction<IEnumerable, RecordValue> fromPairs = new FromPairs();

        var result = fromPairs.Evaluate(pairs.Evaluate(input));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Keys, Is.EqualTo(input.Keys));
            Assert.That(result["_id"], Is.SameAs(nested));
            Assert.That(result.ContainsKey("missing"), Is.True);
            Assert.That(result["missing"], Is.Null);
            Assert.That(result["name"], Is.EqualTo("Alice"));
        }
    }

    [Test]
    public void FromPairs_SupportsLazyEnumerable()
    {
        IEnumerable<PairValue> Input()
        {
            yield return new PairValue(2025m, 100m);
            yield return new PairValue("_id", null);
        }

        var result = new FromPairs().Evaluate(Input());

        Assert.That(result.Keys, Is.EqualTo(new[] { "2025", "_id" }));
        Assert.That(result["2025"], Is.EqualTo(100m));
        Assert.That(result.ContainsKey("_id"), Is.True);
    }

    [Test]
    public void Pairs_SupportsHostDictionary()
    {
        var input = new Dictionary<string, object?> { ["_id"] = 42m, ["value"] = null };
        var result = ((IFunction)new Pairs()).Evaluate(input);

        Assert.That(result, Is.EqualTo(new[] { new PairValue("_id", 42m), new PairValue("value", null) }));
    }

    [TestCase("pairs", "record", "array")]
    [TestCase("from-pairs", "array", "record")]
    public void Operators_ExposeCatalogContracts(string name, string input, string output)
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Input, Is.EqualTo(input));
            Assert.That(info.Output, Is.EqualTo(output));
            Assert.That(info.Scope, Is.EqualTo("record"));
            Assert.That(info.Aliases, Is.Empty);
            Assert.That(info.Converted, Is.True);
        }
    }

    [TestCase("pairs(\"key\", \"value\")")]
    [TestCase("from-pairs(\"key\", \"value\")")]
    public void Operators_RejectLegacyParameters(string expression)
        => Assert.That(() => Expression.Create(expression), Throws.InstanceOf<BindingException>());

    private sealed class UncoercibleKey
    {
        public override string? ToString() => null;
    }
}
