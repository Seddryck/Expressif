using Expressif.Accumulators;
using Expressif.Accumulators.Introspection;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Accumulators.Array;

[TestFixture]
public class CommonAffixTest
{
    [Conformance]
    public void CommonPrefix_Values(object?[] value, string? expected)
        => Assert.That(Accumulate(new CommonPrefixAccumulator(), value), Is.EqualTo(expected));

    [Conformance]
    public void CommonSuffix_Values(object?[] value, string? expected)
        => Assert.That(Accumulate(new CommonSuffixAccumulator(), value), Is.EqualTo(expected));

    [TestCase("common-prefix")]
    [TestCase("common-suffix")]
    public void Initialize_AfterAccumulation_ResetsState(string name)
    {
        var accumulator = AccumulatorFactory.Instantiate(name);
        Accumulate(accumulator, ["abc", "xyz"]);
        accumulator.Initialize();
        Assert.That(accumulator.GetValue(), Is.Null);
        accumulator.Accumulate("fresh");
        Assert.That(accumulator.GetValue(), Is.EqualTo("fresh"));
    }

    [TestCase("common-prefix", null)]
    [TestCase("common-prefix", 42)]
    [TestCase("common-prefix", true)]
    [TestCase("common-suffix", null)]
    [TestCase("common-suffix", 42)]
    [TestCase("common-suffix", true)]
    public void Accumulate_InvalidItem_ThrowsBeforeAndAfterEmptyResult(string name, object? item)
    {
        var accumulator = AccumulatorFactory.Instantiate(name);
        accumulator.Initialize();
        Assert.That(() => accumulator.Accumulate(item), Throws.TypeOf<InvalidCastException>());
        accumulator.Accumulate("abc");
        Assert.That(() => accumulator.Accumulate(item), Throws.TypeOf<InvalidCastException>());
        accumulator.Accumulate("xyz");
        Assert.That(() => accumulator.Accumulate(item), Throws.TypeOf<InvalidCastException>());
    }

    [TestCase("common-prefix", "{\"interact\", \"internet\", \"internal\"}", "inter", new[] { "interact", "inter", "inter" })]
    [TestCase("common-suffix", "{\"running\", \"walking\", \"talking\"}", "ing", new[] { "running", "ing", "ing" })]
    public void Evaluate_Composition_UsesAccumulator(string name, string source, string expected, string[] running)
    {
        Assert.That(Expression.CreateClosed($"{source} | fold({name})").Evaluate(null), Is.EqualTo(expected));
        Assert.That(Expression.CreateClosed($"{source} | scan({name})").Evaluate(null), Is.EqualTo(running));
        Assert.That(Expression.CreateClosed($"{source} | broadcast({name})").Evaluate(null), Is.EqualTo(new[] { expected, expected, expected }));
        Assert.That(Expression.CreateClosed($"{{}} | fold({name})").Evaluate(null), Is.Null);
    }

    [TestCase("common-prefix")]
    [TestCase("common-suffix")]
    public void Locate_CanonicalName_ExposesMetadata(string name)
    {
        var info = new AccumulatorIntrospector().Describe().Single(x => x.Name == name);
        Assert.That(info.Aliases, Does.Contain(name));
        Assert.That(info.Scope, Is.EqualTo("Array"));
        Assert.That(AccumulatorFactory.Instantiate(name), Is.TypeOf(info.ImplementationType));
    }

    private static object? Accumulate(IAccumulator accumulator, object?[] values)
    {
        accumulator.Initialize();
        foreach (var value in values)
            accumulator.Accumulate(value);
        return accumulator.GetValue();
    }
}
