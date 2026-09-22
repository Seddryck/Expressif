using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Array.Partitioning;

[TestFixture]
public class DistributeRandomSplitTest
{
    [Conformance]
    public void DistributeRandomSplit_Valid_Weights(object? input, string weights, int seed, string? expected)
        => AssertConformance(input, weights, seed, expected);

    [Conformance]
    public void DistributeRandomSplit_Seeded_Assignment(object? input, string weights, int seed, string? expected)
        => AssertConformance(input, weights, seed, expected);

    [TestCase("{}")]
    [TestCase("{0, 0}")]
    [TestCase("{1, -1}")]
    [TestCase("{1, #null}")]
    [TestCase("{1, \"invalid\"}")]
    public void Expression_InvalidWeights_ReturnsNull(string weights)
        => Assert.That(TestExpression.Create($"distribute-random-split({weights}, 42)").Evaluate(new[] { 1, 2 }), Is.Null);

    [Test]
    public void Expression_SameSeed_ReproducesAssignment()
    {
        var expression = TestExpression.Create("distribute-random-split({1, 3}, 42)");
        var input = Enumerable.Range(1, 50).ToArray();
        var first = expression.Evaluate(input);
        var second = expression.Evaluate(input);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Expression_OmittedSeed_UsesRandomAssignment()
        => Assert.That(
            TestExpression.Create("distribute-random-split({1, 0})").Evaluate(new[] { 1, 2 }),
            Is.EqualTo(new object?[][] { [1, 2], [] }));

    private static void AssertConformance(object? input, string weights, int seed, string? expected)
    {
        if (input is "(null)")
        {
            Assert.That(TestExpression.Create($"distribute-random-split({weights}, {seed})").Evaluate(null), Is.Null);
            return;
        }

        var value = input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = TestExpression.Create($"distribute-random-split({weights}, {seed})").Evaluate(value);
        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }
}
