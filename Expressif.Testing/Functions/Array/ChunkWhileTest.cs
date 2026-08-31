using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class ChunkWhileTest
{
    [Conformance]
    public void ChunkWhile_Valid_Operation(object? input, string operation, string? expected)
    {
        if (input is "(null)")
        {
            Assert.That(new ChunkWhile(() => throw new InvalidOperationException()).Evaluate(null), Is.Null);
            return;
        }

        var value = input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = Expression.Create($"chunk-while({operation})").Evaluate(value);
        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Expression_NumericGaps_FormsConsecutiveChunks()
        => Assert.That(
            Expression.Create("chunk-while(subtract | is-less-than(2))")
                .Evaluate(new[] { 10, 20, 21, 22, 30, 31 }),
            Is.EqualTo(new object?[][] { [10], [20, 21, 22], [30, 31] }));

    [Test]
    public void Evaluate_NonBooleanOperation_ReturnsNull()
        => Assert.That(Expression.Create("chunk-while(subtract)").Evaluate(new[] { 1, 2 }), Is.Null);
}
