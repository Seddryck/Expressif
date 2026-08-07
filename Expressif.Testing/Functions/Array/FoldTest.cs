using Expressif.Functions.Array;
using Expressif.Accumulators;

namespace Expressif.Testing.Functions.Array;

public class FoldTest
{
    [Test]
    public void Evaluate_CountAccumulator_Valid()
        => Assert.That(new Fold(() => new CountAccumulator()).Evaluate(new object?[] { 1, "2", null }), Is.EqualTo(3));

    [Test]
    public void Evaluate_SumAccumulator_WithCastableValues_Valid()
        => Assert.That(new Fold(() => new SumAccumulator()).Evaluate(new object[] { 1, "2", true }), Is.EqualTo(4m));

    [Test]
    public void Evaluate_MinAccumulator_WithCastableValues_Valid()
        => Assert.That(new Fold(() => new MinAccumulator()).Evaluate(new object[] { "10", 4, 8.5m }), Is.EqualTo(4m));

    [Test]
    public void Evaluate_MaxAccumulator_WithCastableValues_Valid()
        => Assert.That(new Fold(() => new MaxAccumulator()).Evaluate(new object[] { "10", 4, 8.5m }), Is.EqualTo(10m));

    [Test]
    public void Evaluate_FirstAccumulator_Valid()
        => Assert.That(new Fold(() => new FirstAccumulator()).Evaluate(new object[] { 3, 2, 1 }), Is.EqualTo(3));

    [Test]
    public void Evaluate_LastAccumulator_Valid()
        => Assert.That(new Fold(() => new LastAccumulator()).Evaluate(new object[] { 3, 2, 1 }), Is.EqualTo(1));

    [TestCaseSource(nameof(BooleanAccumulatorCases))]
    public void Evaluate_BooleanAccumulator_Valid(IAccumulator accumulator, object[] input, bool expected)
        => Assert.That(new Fold(() => accumulator).Evaluate(input), Is.EqualTo(expected));

    private static readonly object[] BooleanAccumulatorCases =
    [
        new object[] { new EveryAccumulator(), new object[] { true, true, true }, true },
        new object[] { new EveryAccumulator(), new object[] { true, false, true }, false },
        new object[] { new AnyAccumulator(), new object[] { false, true, false }, true },
        new object[] { new AnyAccumulator(), new object[] { false, false, false }, false }
    ];

    [Test]
    public void Evaluate_EmptyArray_ExpectedDefaults()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Fold(() => new CountAccumulator()).Evaluate(System.Array.Empty<object>()), Is.EqualTo(0));
            Assert.That(new Fold(() => new SumAccumulator()).Evaluate(System.Array.Empty<object>()), Is.EqualTo(0m));
            Assert.That(new Fold(() => new MinAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new MaxAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new FirstAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new LastAccumulator()).Evaluate(System.Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new EveryAccumulator()).Evaluate(System.Array.Empty<object>()), Is.True);
            Assert.That(new Fold(() => new AnyAccumulator()).Evaluate(System.Array.Empty<object>()), Is.False);
        });
    }

    [TestCaseSource(nameof(BooleanAccumulators))]
    public void Evaluate_BooleanAccumulatorWithNull_ThrowsInvalidCastException(IAccumulator accumulator)
        => Assert.Throws<InvalidCastException>(() => new Fold(() => accumulator).Evaluate(new object?[] { null }));

    [TestCaseSource(nameof(BooleanAccumulators))]
    public void Evaluate_BooleanAccumulatorWithInvalidValue_ThrowsInvalidCastException(IAccumulator accumulator)
        => Assert.Throws<InvalidCastException>(() => new Fold(() => accumulator).Evaluate(new object[] { "not-a-boolean" }));

    private static readonly IAccumulator[] BooleanAccumulators = [new EveryAccumulator(), new AnyAccumulator()];

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Fold(() => new SumAccumulator()).Evaluate(10), Is.Null);

    [Test]
    public void Evaluate_StringArrayLiteralInput_Valid()
        => Assert.That(new Fold(() => new SumAccumulator()).Evaluate("{1,2,2}"), Is.EqualTo(5m));
}
