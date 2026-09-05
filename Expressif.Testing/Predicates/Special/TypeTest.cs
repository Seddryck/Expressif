using Expressif.Predicates;
using Expressif.Predicates.Special;
using Expressif.Testing.Conformance;
using Expressif.Types;
using Expressif.Values;

namespace Expressif.Testing.Predicates.Special;

public class TypeTest
{
    [Conformance]
    public void IsType_Text(string? value, string type, bool expected)
        => Assert.That(Evaluate(value, type), Is.EqualTo(expected));

    [Conformance]
    public void IsType_Integer(int value, string type, bool expected)
        => Assert.That(Evaluate(value, type), Is.EqualTo(expected));

    [Conformance]
    public void IsType_Decimal(decimal value, string type, bool expected)
        => Assert.That(Evaluate(value, type), Is.EqualTo(expected));

    [Test]
    public void Evaluate_TemporalTypesAndFamilies_AreStrict()
        => Assert.Multiple(() =>
        {
            Assert.That(Evaluate(new DateOnly(2026, 8, 30), "temporal"), Is.True);
            Assert.That(Evaluate(new DateOnly(2026, 8, 30), "datetime"), Is.False);
            Assert.That(Evaluate(new DateTime(2026, 8, 30, 14, 30, 0), "date"), Is.False);
            Assert.That(Evaluate(new TimeOnly(14, 30), "datetime"), Is.False);
            Assert.That(Evaluate(TimeSpan.FromHours(3), "duration"), Is.True);
            Assert.That(Evaluate(TimeSpan.FromHours(3), "temporal"), Is.False);
        });

    [Test]
    public void Evaluate_StructuredTypes_AreNotInterpretedAsEachOther()
        => Assert.Multiple(() =>
        {
            Assert.That(Evaluate(new object?[] { 1, 2 }, "array"), Is.True);
            Assert.That(Evaluate(new object?[] { 1, 2 }, "tuple"), Is.False);
            Assert.That(Evaluate(new TupleValue(1, 2), "tuple"), Is.True);
            Assert.That(Evaluate(new TupleValue(1, 2), "array"), Is.False);
            Assert.That(Evaluate(new PairValue("USA", 42), "pair"), Is.True);
            Assert.That(Evaluate(new PairValue("USA", 42), "tuple"), Is.True);
            Assert.That(Evaluate(new Group("USA", new[] { 1, 2 }), "tuple"), Is.True);
            Assert.That(Evaluate(new RecordValue(), "record"), Is.True);
        });

    [Test]
    public void Factory_TypeLiteral_BindsThroughCanonicalRegistry()
    {
        var predicate = new PredicationFactory().Instantiate("is-type(:integer)", new Context());

        Assert.Multiple(() =>
        {
            Assert.That(predicate.Evaluate(42), Is.True);
            Assert.That(predicate.Evaluate(42m), Is.False);
            Assert.That(predicate.Evaluate("42"), Is.False);
        });
    }

    [Test]
    public void Evaluate_Null_DoesNotMatchNonNullDescriptors()
        => Assert.Multiple(() =>
        {
            Assert.That(Evaluate(null, "numeric"), Is.False);
            Assert.That(Evaluate(null, "text"), Is.False);
            Assert.That(Evaluate(null, "record"), Is.False);
        });

    [Test]
    public void Factory_UnknownTypeLiteral_UsesNormalBindingError()
        => Assert.That(
            () => new PredicationFactory().Instantiate("is-type(:unknown)", new Context()),
            Throws.TypeOf<UnknownExpressifTypeException>());

    private static bool Evaluate(object? value, string type)
        => new IsType(() => TypeRegistry.Resolve(type)).Evaluate(value);
}
