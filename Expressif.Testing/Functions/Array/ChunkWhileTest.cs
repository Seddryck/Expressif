using Expressif.Functions;
using Expressif.Functions.Array;
using Moq;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class ChunkWhileTest
{
    [Conformance]
    public void ChunkWhile_ExplicitBinding(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value is string source ? new ParameterValueConverter().Parse(source) : value), Is.EqualTo(Expression.Create(expected).Evaluate(null)));

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
            Expression.Create("chunk-while($1 | subtract($0 | last) | is-less-than(2))")
                .Evaluate(new[] { 10, 20, 21, 22, 30, 31 }),
            Is.EqualTo(new object?[][] { [10], [20, 21, 22], [30, 31] }));

    [Test]
    public void Evaluate_NonBooleanOperation_ReturnsNull()
        => Assert.That(Expression.Create("chunk-while($0 | cardinality)").Evaluate(new[] { 1, 2 }), Is.Null);

    [Test]
    public void Evaluate_Operation_ReceivesStableChunkAndCandidateOnce()
    {
        var observed = new List<IPositionalValue>();
        var operation = new Mock<IFunction>();
        operation.Setup(x => x.Evaluate(It.IsAny<object?>())).Returns<object?>(value =>
        {
            var pair = (IPositionalValue)value!;
            observed.Add(pair);
            return ((object?[])pair.GetPosition(0)!).Length < 2;
        });
        var function = new ChunkWhile(() => operation.Object);
        Assert.That(function.Evaluate(new[] { 1, 2, 3, 4, 5 }),
            Is.EqualTo(new object?[][] { [1, 2], [3, 4], [5] }));
        Assert.That(observed.Select(pair => pair.GetPosition(0)),
            Is.EqualTo(new object?[][] { [1], [1, 2], [3], [3, 4] }));
        Assert.That(observed.Select(pair => pair.GetPosition(1)), Is.EqualTo(new[] { 2, 3, 4, 5 }));
        operation.Verify(x => x.Evaluate(It.IsAny<object?>()), Times.Exactly(4));
    }

    [Test]
    public void Evaluate_EmptySingletonAndUnsupported_DoNotCreateOperation()
    {
        var function = new ChunkWhile(() => throw new InvalidOperationException());
        Assert.Multiple(() =>
        {
            Assert.That(function.Evaluate(System.Array.Empty<object?>()), Is.Empty);
            Assert.That(function.Evaluate(new object?[] { null }), Is.EqualTo(new object?[][] { [null] }));
            Assert.That(function.Evaluate(42), Is.Null);
            Assert.That(function.Evaluate("hello"), Is.Null);
            Assert.That(function.Evaluate(null), Is.Null);
        });
    }

    [TestCase(null)]
    [TestCase("true")]
    [TestCase(1)]
    public void Evaluate_NonBooleanResult_StopsImmediately(object? result)
    {
        var operation = new Mock<IFunction>();
        operation.Setup(x => x.Evaluate(It.IsAny<object?>())).Returns(result);
        Assert.That(new ChunkWhile(() => operation.Object).Evaluate(new[] { 1, 2, 3 }), Is.Null);
        operation.Verify(x => x.Evaluate(It.IsAny<object?>()), Times.Once);
    }

    [Test]
    public void Expression_ReusedConcurrently_IsIsolated()
    {
        var expression = Expression.Create("chunk-while($0 | cardinality | less-than(2))");
        Parallel.For(0, 20, index =>
        {
            Assert.That(expression.Evaluate(new[] { index, index + 1, index + 2 }),
                Is.EqualTo(new object?[][] { [index, index + 1], [index + 2] }));
        });
    }
}
