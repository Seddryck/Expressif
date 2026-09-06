using System.Collections;
using Expressif.Functions;
using Expressif.Functions.Array;
using Expressif.Functions.Introspection;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Array;

public class ExpressionSelectionTest
{
    [Conformance]
    public void MinBy_Selection(object? input, string expression, decimal? expected)
        => AssertSelection(input, expression, expected);

    [Conformance]
    public void MaxBy_Selection(object? input, string expression, decimal? expected)
        => AssertSelection(input, expression, expected);

    [Conformance]
    public void ClosestBy_Selection(object? input, string expression, decimal? expected)
        => AssertSelection(input, expression, expected);

    private static void AssertSelection(object? input, string expression, decimal? expected)
    {
        var result = Expression.Create(expression).Evaluate(input);
        Assert.That(result is null ? (decimal?)null : Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(expected));
    }

    [TestCase("min-by(.bar)", 1)]
    [TestCase("max-by(.bar)", 4)]
    [TestCase("closest-by(.bar, 32)", 2)]
    [TestCase("max-by(.bar | subtract(40) | absolute)", 1)]
    [TestCase("closest-by(.bar | multiply(2), 75)", 2)]
    [TestCase("closest-by(target := 32, expression := .bar)", 2)]
    public void Evaluate_RecordExpression_ReturnsSelectedRecord(string function, int expected)
    {
        var expression = Expression.Create("{{id := 1, bar := 10}, {id := 2, bar := 30}, {id := 3, bar := 30}, {id := 4, bar := 50}} | " + function + " | .id");
        Assert.That(expression.Evaluate(null), Is.EqualTo(expected));
    }

    [TestCase("min-by", "a")]
    [TestCase("max-by", "z")]
    public void Evaluate_TextCriteria_UsesOrdinalOrder(string name, string expected)
        => Assert.That(Expression.Create(name + "(neutral)").Evaluate(new[] { "z", "a" }), Is.EqualTo(expected));

    [TestCase("min-by")]
    [TestCase("max-by")]
    [TestCase("closest-by")]
    public void Evaluate_VisitsOnceAndPreservesReference(string name)
    {
        var calls = 0;
        var first = new object();
        var selector = new DelegatedFunction(value => { calls++; return 3m; });
        IFunction function = name switch
        {
            "min-by" => new MinBy(() => selector),
            "max-by" => new MaxBy(() => selector),
            _ => new ClosestBy(() => selector, () => 3m),
        };
        Assert.That(function.Evaluate(new SinglePass(first, new object())), Is.SameAs(first));
        Assert.That(calls, Is.EqualTo(2));
    }

    [Test]
    public void ClosestBy_ExtremeDistances_AreExact()
    {
        var function = new ClosestBy(() => new DelegatedFunction(value => value), () => decimal.MaxValue);
        Assert.That(function.Evaluate(new[] { decimal.MinValue, 0m }), Is.EqualTo(0m));
    }

    [TestCase("min-by(neutral)")]
    [TestCase("max-by(neutral)")]
    public void Evaluate_MixedNumericTypes_AreComparable(string expression)
    {
        var result = Expression.Create(expression).Evaluate(new object[] { 2, 3m, 1.0 });
        Assert.That(result, Is.EqualTo(expression.StartsWith("min", StringComparison.Ordinal) ? 1 : 3));
    }

    [TestCase("min-by(neutral)")]
    [TestCase("max-by(neutral)")]
    [TestCase("closest-by(neutral, 2)")]
    public void Evaluate_InvalidCriteria_Fail(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(new[] { new object() }), Throws.ArgumentException);

    [TestCase("min-by")]
    [TestCase("max-by")]
    [TestCase("closest-by")]
    public void Introspection_DescribesElementSelection(string name)
    {
        var info = new FunctionIntrospector().Describe().Single(function => function.Name == name);
        Assert.Multiple(() =>
        {
            Assert.That(info.Input, Is.EqualTo("array"));
            Assert.That(info.Output, Is.EqualTo("any"));
            Assert.That(info.Parameters[0].Type, Is.EqualTo("expression"));
            Assert.That(info.Parameters[0].Name, Is.EqualTo("expression"));
            Assert.That(info.Scope, Is.EqualTo("array/selection"));
        });
    }

    [Test]
    public void Evaluate_SharedExpression_IsConcurrentAndReusable()
    {
        var expression = Expression.Create("max-by(absolute)");
        Parallel.For(1, 50, index => Assert.That(expression.Evaluate(new[] { -index, 0 }), Is.EqualTo(-index)));
    }

    [Test]
    public void ClosestBy_EquidistantCriteria_PreservesFirst()
        => Assert.That(Expression.Create("closest-by(neutral, 3)").Evaluate(new[] { 4, 2 }), Is.EqualTo(4));

    [Test]
    public void ClosestBy_TextCriterion_IsRejected()
        => Assert.That(() => Expression.Create("closest-by(neutral, 3)").Evaluate(new[] { "3" }), Throws.ArgumentException);

    [TestCase("min-by(neutral)")]
    [TestCase("max-by(neutral)")]
    public void Evaluate_IncompatibleCriteria_Fail(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(new object[] { 1, "a" }), Throws.ArgumentException);

    [Test]
    public void Evaluate_DateCriteria_PreserveOriginalValue()
    {
        var first = new DateOnly(2026, 1, 1);
        Assert.That(Expression.Create("min-by(neutral)").Evaluate(new[] { first.AddDays(1), first }), Is.EqualTo(first));
    }

    private sealed class DelegatedFunction(Func<object?, object?> evaluate) : IFunction
    {
        public object? Evaluate(object? value) => evaluate(value);
    }

    private sealed class SinglePass(params object[] values) : IEnumerable
    {
        private bool enumerated;
        public IEnumerator GetEnumerator()
        {
            if (enumerated)
                throw new InvalidOperationException("The source was enumerated twice.");
            enumerated = true;
            return values.GetEnumerator();
        }
    }
}
