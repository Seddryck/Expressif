using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

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
        => Assert.That(Expression.Create($"distribute-random-split({weights}, 42)").Evaluate(new[] { 1, 2 }), Is.Null);

    [Test]
    public void Expression_SameSeed_ReproducesAssignment()
    {
        var expression = Expression.Create("distribute-random-split({1, 3}, 42)");
        var input = Enumerable.Range(1, 50).ToArray();
        var first = expression.Evaluate(input);
        var second = expression.Evaluate(input);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Expression_OmittedSeed_UsesRandomAssignment()
        => Assert.That(
            Expression.Create("distribute-random-split({1, 0})").Evaluate(new[] { 1, 2 }),
            Is.EqualTo(new object?[][] { [1, 2], [] }));

    private static void AssertConformance(object? input, string weights, int seed, string? expected)
    {
        if (input is "(null)")
        {
            Assert.That(Expression.Create($"distribute-random-split({weights}, {seed})").Evaluate(null), Is.Null);
            return;
        }

        var value = input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = Expression.Create($"distribute-random-split({weights}, {seed})").Evaluate(value);
        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }
}
