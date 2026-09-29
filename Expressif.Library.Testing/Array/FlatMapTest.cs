using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using System.Collections;
using Expressif.Functions;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array;

public class FlatMapTest
{
    [Conformance]
    public void FlatMap_Expression(string value, string expression, string expected)
        => Assert.That(
            TestExpression.Create(expression).Evaluate(TestExpression.Create(value).Evaluate(null)),
            Is.EqualTo(TestExpression.Create(expected).Evaluate(null)));

    [TestCase("{1, 2}", "flat-map(add(1))")]
    [TestCase("{\"one\"}", "flat-map(upper)")]
    [TestCase("{{orders := #null}}", "flat-map(.orders)")]
    public void Expression_NonArrayResult_Throws(string value, string expression)
        => Assert.Throws<ArgumentException>(() =>
            TestExpression.Create(expression).Evaluate(TestExpression.Create(value).Evaluate(null)));

    [Test]
    public void Evaluate_TypedContract_VisitsEachElementOnceInOrder()
    {
        var visits = new List<object?>();
        IFunction<IEnumerable, IEnumerable?> function = new FlatMap(() => new DelegatedFunction(value =>
        {
            visits.Add(value);
            return new object?[] { value, value };
        }));

        Assert.That(function.Evaluate(new[] { 1, 2, 3 }), Is.EqualTo(new[] { 1, 1, 2, 2, 3, 3 }));
        Assert.That(visits, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Evaluate_EmptyInput_DoesNotEvaluateExpression()
    {
        var function = new FlatMap(() => new DelegatedFunction(_ => throw new InvalidOperationException()));
        Assert.That(function.Evaluate(System.Array.Empty<object>()), Is.Empty);
    }

    [Test]
    public void Expression_NestedArgumentContext_ReadsSourceElement()
        => Assert.That(
            TestExpression.Create("flat-map(.text | tokenize(.separator))").Evaluate(
                TestExpression.Create("{{text := \"a,b\", separator := \",\"}, {text := \"c d\", separator := \" \"}}").Evaluate(null)),
            Is.EqualTo(new[] { "a", "b", "c", "d" }));

    private sealed class DelegatedFunction(Func<object?, object?> implementation) : IFunction
    {
        public object? Evaluate(object? value) => implementation(value);
    }
}
