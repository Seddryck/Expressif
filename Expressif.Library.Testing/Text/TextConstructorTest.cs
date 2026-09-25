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
using Expressif.Library.Array;
using Expressif.Testing.Conformance;
using TextFunction = Expressif.Library.Text.Concatenation.Text;

namespace Expressif.Testing.Text;

[TestFixture]
public class TextConstructorTest
{
    [Conformance]
    public void Text_Valid_VariadicValues(object? input, string expression, string expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Evaluate_Arguments_EvaluatesOnceFromLeftToRightAgainstSameInput()
    {
        var order = new List<string>();
        var input = new object();
        var function = new TextFunction(value =>
        {
            Assert.That(value, Is.SameAs(input));
            order.Add("a");
            order.Add("b");
            order.Add("c");
            return ["one", 2, "three"];
        });

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
            TestExpression.Create("text(\"foo\", ...@names, \"bar\")", context).Evaluate(null),
            Is.EqualTo("fooNikolaTeslabar"));
    }

    [Test]
    public void Expression_SpreadPipeline_AppliesToSpreadSource()
        => Assert.That(
            TestExpression.Create("text(\"foo\", ...({\"Nikola\", \"Tesla\"} |> prepend-space))").Evaluate(null),
            Is.EqualTo("foo Nikola Tesla"));

    [Test]
    public void Expression_SpreadMappedValues_MapsBeforeExpansion()
        => Assert.That(
            TestExpression.Create("text(...({\"nikola\", \"tesla\"} |> upper))").Evaluate(null),
            Is.EqualTo("NIKOLATESLA"));

    [Test]
    public void Evaluate_NonSpreadArray_RemainsSingleValue()
    {
        var array = new object?[] { "Nikola", "Tesla" };
        var function = new TextFunction(_ => [array]);

        Assert.That(function.Evaluate(null), Is.EqualTo(array.ToString()));
    }

    [TestCase("text(...#null)", "Spread argument cannot be null.")]
    [TestCase("text(...42)", "Spread argument must evaluate to an array.")]
    [TestCase("text(...\"abc\")", "Spread argument must evaluate to an array.")]
    public void Expression_InvalidSpread_ThrowsSpecificError(string source, string message)
        => Assert.That(
            () => TestExpression.Create(source).Evaluate(null),
            Throws.TypeOf<SpreadArgumentException>().With.Message.EqualTo(message));
}
