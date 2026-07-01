using Expressif.Functions.Array;
using Expressif.Accumulators;

namespace Expressif.Testing.Functions.Aggregation;

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

    [Test]
    public void Evaluate_EmptyArray_ExpectedDefaults()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Fold(() => new CountAccumulator()).Evaluate(Array.Empty<object>()), Is.EqualTo(0));
            Assert.That(new Fold(() => new SumAccumulator()).Evaluate(Array.Empty<object>()), Is.EqualTo(0m));
            Assert.That(new Fold(() => new MinAccumulator()).Evaluate(Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new MaxAccumulator()).Evaluate(Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new FirstAccumulator()).Evaluate(Array.Empty<object>()), Is.Null);
            Assert.That(new Fold(() => new LastAccumulator()).Evaluate(Array.Empty<object>()), Is.Null);
        });
    }

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Fold(() => new SumAccumulator()).Evaluate(10), Is.Null);
}
