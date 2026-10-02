using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Partitioning;

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
        => Assert.That(TestExpression.Create("chunk(2)").Evaluate(new[] { 1, 2, 3 }),
            Is.EqualTo(new object?[][] { [1, 2], [3] }));

    [Test]
    public void Expression_ParameterExpression_EvaluatesSizeFromContext()
    {
        var context = new Context();
        context.Variables.Add<int>("size", 1);

        Assert.That(TestExpression.Create("chunk({@size | increment})", context).Evaluate(new[] { 1, 2, 3 }),
            Is.EqualTo(new object?[][] { [1, 2], [3] }));
    }

    [Test]
    public void Evaluate_NonEnumerable_ReturnsNull()
        => Assert.That(new Chunk(() => 2).Evaluate(42), Is.Null);
}
