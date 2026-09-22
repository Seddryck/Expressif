using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Testing.Conformance;
using Expressif.Library.Array;
using System.Text.Json;
using ArrayFunction = Expressif.Library.Array.Array;

namespace Expressif.Testing.Array;

[TestFixture]
public class ArrayTest
{
    [Conformance]
    public void Array_Valid_VariadicValues(object? input, string expression, string expected)
        => Assert.That(JsonSerializer.Serialize(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Evaluate_ArgumentsAndSpreads_EvaluatesOnceFromLeftToRight()
    {
        var order = new List<string>();
        var function = new ArrayFunction(() =>
        [
            new(input => { order.Add("a"); return 1; }),
            new(input => { order.Add("b"); return new[] { 2, 3 }; }, true),
            new(input => { order.Add("c"); return 4; }),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(function.Evaluate(null), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            Assert.That(order, Is.EqualTo(new[] { "a", "b", "c" }));
        });
    }

    [Test]
    public void Expression_VariableSpread_ExpandsInPlace()
    {
        var context = new Context();
        context.Variables.Add<int[]>("values", new[] { 2, 3 });

        Assert.That(
            TestExpression.Create("array(1, ...@values, 4)", context).Evaluate(null),
            Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void Expression_MultipleSpreads_ExpandInPlace()
        => Assert.That(
            TestExpression.Create("array(...{1, 2}, ...{3, 4})").Evaluate(null),
            Is.EqualTo(new[] { 1, 2, 3, 4 }));

    [Test]
    public void Expression_VariablePipeline_ImplicitSpread_ExpandsCurrentInput()
    {
        var context = new Context();
        context.Variables.Add<int[]>("items", new[] { 1, 2, 3 });

        Assert.That(
            TestExpression.Create("@items | array(0, ..., 4)", context).Evaluate(null),
            Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
    }

    [Test]
    public void Expression_VariablePipeline_ExplicitCurrentInputSpread_ExpandsCurrentInput()
    {
        var context = new Context();
        context.Variables.Add<int[]>("items", new[] { 1, 2, 3 });

        Assert.That(
            TestExpression.Create("@items | array(0, ...@_, 4)", context).Evaluate(null),
            Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
    }

    [Test]
    public void Expression_ArrayLiteralPipeline_ImplicitSpread_ExpandsCurrentInput()
        => Assert.That(
            TestExpression.Create("{1, 2, 3} | array(0, ..., 4)").Evaluate(null),
            Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));

    [Test]
    public void Expression_VariablePipeline_ComputedSpread_EvaluatesAgainstCurrentInput()
    {
        var context = new Context();
        context.Variables.Add<int[]>("items", new[] { 1, 2, 3 });

        Assert.That(
            TestExpression.Create("@items | array(0, ...(filter(greater-than(1))), 4)", context).Evaluate(null),
            Is.EqualTo(new[] { 0, 2, 3, 4 }));
    }

    [Test]
    public void Expression_SpreadMappedValues_MapsBeforeExpansion()
        => Assert.That(
            TestExpression.Create("array(...({1, 2, 3} |> multiply(10)))").Evaluate(null),
            Is.EqualTo(new[] { 10, 20, 30 }));

    [TestCase("array(...#null)", "Spread argument cannot be null.")]
    [TestCase("array(...42)", "Spread argument must evaluate to an array.")]
    [TestCase("array(...\"abc\")", "Spread argument must evaluate to an array.")]
    public void Expression_InvalidSpread_ThrowsSpecificError(string source, string message)
        => Assert.That(
            () => TestExpression.Create(source).Evaluate(null),
            Throws.TypeOf<SpreadArgumentException>().With.Message.EqualTo(message));

    [Test]
    public void ArrayLiteral_Spread_ExpandsInPlace()
        => Assert.That(
            TestExpression.CreateClosed("{1, ...{2, 3}, 4}").Evaluate(null),
            Is.EqualTo(new[] { 1, 2, 3, 4 }));

    [TestCase("{before := 1, ..., after := 2}", "Record literal spread entries must specify a field name.")]
    [TestCase("{field := ...@value}", "Record literal field 'field' does not support spread values.")]
    public void RecordLiteral_Spread_ThrowsBindingError(string source, string message)
        => Assert.That(
            () => TestExpression.CreateClosed(source),
            Throws.TypeOf<Expressif.Bindings.BindingException>().With.Message.EqualTo(message));
}
