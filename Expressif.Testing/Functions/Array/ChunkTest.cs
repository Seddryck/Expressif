using Expressif.Functions.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class ChunkTest
{
    [Conformance]
    public void Chunk_Valid_Size(object input, int size, object? expected)
        => Assert.That(new Chunk(() => size).Evaluate(input), Is.EqualTo(expected));

    [TestCase(0)]
    [TestCase(-1)]
    public void Evaluate_InvalidSize_ReturnsNull(int size)
        => Assert.That(new Chunk(() => size).Evaluate(new[] { 1, 2, 3 }), Is.Null);

    [Test]
    public void Expression_LiteralSize_InstantiatesAndEvaluates()
        => Assert.That(new Expression("chunk(2)").Evaluate(new[] { 1, 2, 3 }),
            Is.EqualTo(new object?[][] { [1, 2], [3] }));

    [Test]
    public void Expression_ParameterExpression_EvaluatesSizeFromContext()
    {
        var context = new Context();
        context.Variables.Add<int>("size", 1);

        Assert.That(new Expression("chunk({@size | increment})", context).Evaluate(new[] { 1, 2, 3 }),
            Is.EqualTo(new object?[][] { [1, 2], [3] }));
    }

    [Test]
    public void Evaluate_NonEnumerable_ReturnsNull()
        => Assert.That(new Chunk(() => 2).Evaluate(42), Is.Null);
}
