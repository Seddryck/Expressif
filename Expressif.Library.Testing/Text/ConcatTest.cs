using Expressif.Library.Text;
using Expressif.Functions.Accumulation;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Text;

[TestFixture]
public class ConcatTest
{
    [Conformance]
    public void Concat_WithoutSeparator(object? value, string expected)
        => Assert.That(Evaluate(value, "concat"), Is.EqualTo(expected));

    [Conformance]
    public void Concat_WithSeparator(object? value, string separator, string expected)
        => Assert.That(Evaluate(value, $"concat(\"{separator}\")"), Is.EqualTo(expected));

    [Test]
    public void Evaluate_NamedSeparator_Valid()
        => Assert.That(
            TestExpression.CreateClosed("{\"a\", \"b\"} | concat(separator := \"-\")").Evaluate(null),
            Is.EqualTo("a-b"));

    [Test]
    public void Evaluate_AfterChars_ReassemblesText()
        => Assert.That(TestExpression.Create("chars | concat").Evaluate("abc"), Is.EqualTo("abc"));

    [Test]
    public void CreateSession_AfterAccumulation_IsolatesState()
    {
        var aggregation = new ConcatAccumulator(() => "-");
        var first = aggregation.CreateSession();
        first.Add("a");
        first.Add("b");
        var second = aggregation.CreateSession();
        second.Add("c");

        Assert.Multiple(() =>
        {
            Assert.That(first.Snapshot(), Is.EqualTo("a-b"));
            Assert.That(second.Snapshot(), Is.EqualTo("c"));
        });
    }

    [Test]
    public void Accumulate_Null_ThrowsInvalidCastException()
    {
        var session = new ConcatAccumulator().CreateSession();

        Assert.That(() => session.Add(null), Throws.TypeOf<InvalidCastException>());
    }

    private static object? Evaluate(object? value, string expression)
    {
        var source = value switch
        {
            "(empty)" => "{}",
            string text => text,
            _ => throw new ArgumentException("Conformance input must use Expressif array syntax.", nameof(value)),
        };
        return TestExpression.CreateClosed($"{source} | {expression}").Evaluate(null);
    }
}
