using Expressif.Accumulators;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Accumulators.Text;

[TestFixture]
public class ConcatTest
{
    [Conformance]
    public void Concat_WithoutSeparator(object? value, string expected)
        => Assert.That(Evaluate(value, "concat"), Is.EqualTo(expected));

    [Conformance]
    public void Concat_WithSeparator(object? value, string separator, string expected)
        => Assert.That(Evaluate(value, $"concat(\"{separator}\")"), Is.EqualTo(expected));

    [Conformance]
    public void Implode_WithoutSeparator(object? value, string expected)
        => Assert.That(Evaluate(value, "implode"), Is.EqualTo(expected));

    [Conformance]
    public void Implode_WithSeparator(object? value, string separator, string expected)
        => Assert.That(Evaluate(value, $"implode(\"{separator}\")"), Is.EqualTo(expected));

    [Test]
    public void Evaluate_NamedSeparator_Valid()
        => Assert.That(
            Expression.CreateClosed("{\"a\", \"b\"} | concat(separator := \"-\")").Evaluate(null),
            Is.EqualTo("a-b"));

    [Test]
    public void Evaluate_AfterChars_ReassemblesText()
        => Assert.That(Expression.Create("chars | concat").Evaluate("abc"), Is.EqualTo("abc"));

    [Test]
    public void Initialize_AfterAccumulation_ResetsState()
    {
        var accumulator = new ConcatAccumulator(() => "-");
        accumulator.Initialize();
        accumulator.Accumulate("a");
        accumulator.Accumulate("b");
        accumulator.Initialize();
        accumulator.Accumulate("c");

        Assert.That(accumulator.GetValue(), Is.EqualTo("c"));
    }

    [Test]
    public void Accumulate_Null_ThrowsInvalidCastException()
    {
        var accumulator = new ConcatAccumulator();
        accumulator.Initialize();

        Assert.That(() => accumulator.Accumulate(null), Throws.TypeOf<InvalidCastException>());
    }

    [TestCase("implode")]
    [TestCase("implode(\"-\")")]
    [TestCase("implode(separator := \"-\")")]
    [TestCase("fold(implode)")]
    [TestCase("fold(\"implode\")")]
    [TestCase("fold(concat)")]
    public void Evaluate_CompatibilityAlias_MatchesCanonical(string expression)
        => Assert.That(Evaluate("{\"a\", \"\", \"b\"}", expression),
            Is.EqualTo(Evaluate("{\"a\", \"\", \"b\"}", expression.Replace("implode", "concat"))));

    private static object? Evaluate(object? value, string expression)
    {
        var source = value switch
        {
            "(empty)" => "{}",
            string text => text,
            _ => throw new ArgumentException("Conformance input must use Expressif array syntax.", nameof(value)),
        };
        return Expression.CreateClosed($"{source} | {expression}").Evaluate(null);
    }
}
