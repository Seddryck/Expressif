using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Character;
using Expressif.Library.Text.Concatenation;
using Expressif.Library.Text.Conversion;
using Expressif.Library.Text.Counting;
using Expressif.Library.Text.Encoding;
using Expressif.Library.Text.Filtering;
using Expressif.Library.Text.Masking;
using Expressif.Library.Text.Normalization;
using Expressif.Library.Text.Padding;
using Expressif.Library.Text.Partitioning;
using Expressif.Library.Text.Selection;
using Expressif.Library.Text.Tokenization;
using Expressif.Functions;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Text.Partitioning;

[TestFixture]
public class SplitLengthsTest
{
    [Conformance]
    public void SplitLengths_Partition(object? value, string expression, string[]? expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_TypedContract_PreservesUtf16CodeUnits()
    {
        IFunction<string?, string[]?> function = new SplitLengths(() => [new(_ => 1)]);
        var result = function.Evaluate("😀a");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result![0], Has.Length.EqualTo(1));
            Assert.That(string.Concat(result), Is.EqualTo("😀a"));
        }
    }

    [Test]
    public void Evaluate_Arguments_ObserveInputInDeclarationOrder()
    {
        var observed = new List<object?>();
        var function = new SplitLengths(() =>
        [
            new(value => { observed.Add(value); return 2; }),
            new(value => { observed.Add(value); return new[] { 1, 2 }; }, true),
        ]);
        Assert.That(function.Evaluate("abcdef"), Is.EqualTo(new[] { "ab", "c", "de", "f" }));
        Assert.That(observed, Is.EqualTo(new[] { "abcdef", "abcdef" }));
    }

    [Test]
    public void Evaluate_NullAndUnsupportedInput()
    {
        var function = new SplitLengths();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(function.Evaluate(null), Is.Empty);
            Assert.That(function.Evaluate(new[] { 1, 2 }), Is.Null);
        }
    }

    [TestCase("split-lengths(lengths := 2)")]
    [TestCase("split-lengths(...2)")]
    public void Expression_InvalidArgumentShape_Throws(string expression)
        => Assert.That(() => TestExpression.Create(expression).Evaluate("abc"), Throws.Exception);

    [Test]
    public void Expression_ReusedConcurrently_IsIsolated()
    {
        var expression = TestExpression.Create("split-lengths(2, 1)");
        Parallel.For(0, 20, _ => Assert.That(expression.Evaluate("abcdef"), Is.EqualTo(new[] { "ab", "c", "def" })));
    }
}
