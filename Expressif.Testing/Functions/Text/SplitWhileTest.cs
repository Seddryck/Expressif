using Expressif.Functions;
using Expressif.Functions.Text;
using Expressif.Testing.Conformance;
using Expressif.Values;
using Moq;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class SplitWhileTest
{
    [Conformance]
    public void SplitWhile_Partition(object? value, string expression, string[]? expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("x")]
    [TestCase("(null)")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    public void Evaluate_ZeroOrOneCharacter_DoesNotCreateOperation(string? value)
    {
        IFunction<string?, string[]?> function = new SplitWhile(() => throw new InvalidOperationException());
        Assert.That(() => function.Evaluate(value), Throws.Nothing);
    }

    [Test]
    public void Evaluate_Operation_ReceivesStableSegmentAndCandidateOnce()
    {
        var observed = new List<IPositionalValue>();
        var operation = new Mock<IFunction>();
        operation.Setup(x => x.Evaluate(It.IsAny<object?>())).Returns<object?>(value =>
        {
            var pair = (IPositionalValue)value!;
            observed.Add(pair);
            return ((string)pair.GetPosition(0)!).Length < 2;
        });
        IFunction<string?, string[]?> function = new SplitWhile(() => operation.Object);
        Assert.That(function.Evaluate("abcde"), Is.EqualTo(new[] { "ab", "cd", "e" }));
        Assert.That(observed.Select(pair => new[] { pair.GetPosition(0), pair.GetPosition(1) }),
            Is.EqualTo(new object?[][] { ["a", "b"], ["ab", "c"], ["c", "d"], ["cd", "e"] }));
        operation.Verify(x => x.Evaluate(It.IsAny<object?>()), Times.Exactly(4));
    }

    [Test]
    public void Evaluate_NonBooleanResult_StopsImmediately()
    {
        var operation = new Mock<IFunction>();
        operation.Setup(x => x.Evaluate(It.IsAny<object?>())).Returns("true");
        Assert.That(new SplitWhile(() => operation.Object).Evaluate("abcd"), Is.Null);
        operation.Verify(x => x.Evaluate(It.IsAny<object?>()), Times.Once);
    }

    [Test]
    public void Evaluate_UnsupportedInput_DoesNotCreateOperation()
        => Assert.That(new SplitWhile(() => throw new InvalidOperationException()).Evaluate(new[] { "a", "b" }), Is.Null);

    [Test]
    public void Evaluate_Utf16Candidates_PreserveSurrogates()
    {
        var operation = new Mock<IFunction>();
        operation.Setup(x => x.Evaluate(It.IsAny<object?>())).Returns(false);
        var result = (string[])new SplitWhile(() => operation.Object).Evaluate("😀a")!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(3));
            Assert.That(result.Select(segment => segment.Length), Is.All.EqualTo(1));
            Assert.That(string.Concat(result), Is.EqualTo("😀a"));
        }
    }

    [Test]
    public void Expression_ReusedConcurrently_IsIsolated()
    {
        var expression = Expression.Create("split-while($0 | length | is-less-than(2))");
        Parallel.For(0, 20, index =>
        {
            var input = index % 2 == 0 ? "abcdef" : "xyz";
            var expected = index % 2 == 0 ? new[] { "ab", "cd", "ef" } : ["xy", "z"];
            Assert.That(expression.Evaluate(input), Is.EqualTo(expected));
        });
    }
}
