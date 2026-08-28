using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using TextFunction = Expressif.Functions.Text.Text;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class TextConstructorTest
{
    [Conformance]
    public void Text_Valid_VariadicValues(object? input, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Evaluate_Arguments_EvaluatesOnceFromLeftToRightAgainstSameInput()
    {
        var order = new List<string>();
        var input = new object();
        var function = new TextFunction(() =>
        [
            new(value => { order.Add("a"); Assert.That(value, Is.SameAs(input)); return "one"; }),
            new(value => { order.Add("b"); Assert.That(value, Is.SameAs(input)); return 2; }),
            new(value => { order.Add("c"); Assert.That(value, Is.SameAs(input)); return "three"; }),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(function.Evaluate(input), Is.EqualTo("one2three"));
            Assert.That(order, Is.EqualTo(new[] { "a", "b", "c" }));
        });
    }

    [Test]
    public void Expression_VariableSpread_ExpandsInPlace()
    {
        var context = new Context();
        context.Variables.Add<string[]>("names", new[] { "Nikola", "Tesla" });

        Assert.That(
            Expression.Create("text(\"foo\", ...@names, \"bar\")", context).Evaluate(null),
            Is.EqualTo("fooNikolaTeslabar"));
    }

    [Test]
    public void Expression_SpreadPipeline_AppliesToSpreadSource()
        => Assert.That(
            Expression.Create("text(\"foo\", ...{\"Nikola\", \"Tesla\"} | prepend-space)").Evaluate(null),
            Is.EqualTo("foo Nikola Tesla"));

    [Test]
    public void Evaluate_NonSpreadArray_RemainsSingleValue()
    {
        var array = new object?[] { "Nikola", "Tesla" };
        var function = new TextFunction(() => [new(_ => array)]);

        Assert.That(function.Evaluate(null), Is.EqualTo(array.ToString()));
    }

    [TestCase("text(...#null)", "Spread argument cannot be null.")]
    [TestCase("text(...42)", "Spread argument must evaluate to an array.")]
    [TestCase("text(...\"abc\")", "Spread argument must evaluate to an array.")]
    public void Expression_InvalidSpread_ThrowsSpecificError(string source, string message)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<SpreadArgumentException>().With.Message.EqualTo(message));
}
