using System.Collections;
using Expressif.Accumulators;
using Expressif.Accumulators.Introspection;
using Expressif.Bindings;
using Expressif.Functions.Numeric;
using Expressif.Functions.Temporal;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Accumulators.Array;

[TestFixture]
public class ClosestTest
{
    [Conformance]
    public void Closest_Selection(string value, string target, string? expected)
    {
        var reference = Expression.CreateClosed(target).Evaluate(null);
        var accumulator = new ClosestAccumulator(() => reference);
        accumulator.Initialize();
        foreach (var item in (IEnumerable)Expression.CreateClosed(value).Evaluate(null)!)
            accumulator.Accumulate(item);

        var result = expected is null ? null : Expression.CreateClosed(expected).Evaluate(null);
        Assert.That(accumulator.GetValue(), Is.EqualTo(result));
        Assert.That(Expression.CreateClosed($"{value} | closest({target})").Evaluate(null), Is.EqualTo(result));
    }

    [TestCase("closest(32)")]
    [TestCase("closest(target := 32)")]
    [TestCase("CLOSEST(32)")]
    [TestCase("closest(32) | add(2)", 32)]
    public void Evaluate_Binding_Valid(string expression, int expected = 30)
        => Assert.That(Expression.Create(expression).Evaluate(new[] { 10, 30, 50 }), Is.EqualTo(expected));

    [Test]
    public void Evaluate_TargetExpression_UsesContext()
    {
        var context = new Context(new Dictionary<string, object?> { ["target"] = 31 });
        var expression = Expression.Create("closest({@target | increment})", context);
        Assert.That(expression.Evaluate(new[] { 10, 30, 50 }), Is.EqualTo(30));
    }

    [Test]
    public void Evaluate_MissingTarget_Throws()
        => Assert.That(() => Expression.Create("closest"), Throws.TypeOf<MissingRequiredParameterException>());

    [Test]
    public void Evaluate_UnknownTargetName_Throws()
        => Assert.That(() => Expression.Create("closest(value := 32)"), Throws.TypeOf<UnknownParameterNameException>());

    [Test]
    public void Initialize_AfterAccumulation_ResetsStateAndTarget()
    {
        object? target = 32;
        var accumulator = new ClosestAccumulator(() => target);
        accumulator.Initialize();
        accumulator.Accumulate(30);
        target = new DateOnly(2024, 1, 12);
        accumulator.Initialize();
        Assert.That(accumulator.GetValue(), Is.Null);
        var date = new DateOnly(2024, 1, 10);
        accumulator.Accumulate(date);
        Assert.That(accumulator.GetValue(), Is.EqualTo(date));
        target = null;
        accumulator.Initialize();
        accumulator.Accumulate(30);
        Assert.That(accumulator.GetValue(), Is.Null);
    }

    [Test]
    public void Accumulate_CoercedInput_PreservesOriginalObject()
    {
        object input = 30;
        var accumulator = new ClosestAccumulator(() => 32m);
        accumulator.Initialize();
        accumulator.Accumulate(input);
        accumulator.Accumulate(50m);
        Assert.That(accumulator.GetValue(), Is.SameAs(input));
    }

    [Test]
    public void Evaluate_ConcurrentCalls_IsolatesState()
    {
        var expression = Expression.Create("closest(32)");
        Parallel.For(0, 100, i =>
            Assert.That(expression.Evaluate(new[] { i, i + 100 }), Is.EqualTo(i)));
    }

    [Test]
    public void Accumulate_TemporalTypes_UsesDurationBetweenCompatibility()
    {
        object[] items = [new DateOnly(2024, 1, 10), new DateTime(2024, 1, 11),
            new DateTimeOffset(2024, 1, 12, 0, 0, 0, TimeSpan.Zero), new YearMonth(2024, 1), "2024-01-13"];
        object[] targets = [new DateOnly(2024, 1, 12), new DateTime(2024, 1, 12),
            new DateTimeOffset(2024, 1, 12, 0, 0, 0, TimeSpan.Zero), new YearMonth(2024, 1), "2024-01-12"];
        foreach (var target in targets)
        {
            var difference = new DurationBetween(() => target);
            var expected = items.Select(item => (Item: item, Distance: difference.Evaluate(item)))
                .Where(pair => pair.Distance is TimeSpan)
                .OrderBy(pair => Math.Abs(((TimeSpan)pair.Distance!).Ticks))
                .Select(pair => pair.Item).FirstOrDefault();
            var accumulator = new ClosestAccumulator(() => target);
            accumulator.Initialize();
            foreach (var item in items)
                accumulator.Accumulate(item);
            Assert.That(accumulator.GetValue(), Is.SameAs(expected));
        }
    }

    [Test]
    public void Accumulate_Overflow_MatchesSubtract()
    {
        var accumulator = new ClosestAccumulator(() => decimal.MinValue);
        accumulator.Initialize();
        Assert.That(() => new Subtract(() => decimal.MinValue).Evaluate(decimal.MaxValue), Throws.TypeOf<OverflowException>());
        Assert.That(() => accumulator.Accumulate(decimal.MaxValue), Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void Describe_Closest_ExposesRequiredTarget()
    {
        var info = new AccumulatorIntrospector().Describe().Single(info => info.Name == "closest");
        Assert.That(info.Scope, Is.EqualTo("Array"));
        Assert.That(info.Summary, Is.EqualTo("Returns the first non-null input value with the smallest absolute distance to the target."));
        Assert.That(info.Parameters, Has.Length.EqualTo(1));
        Assert.That(info.Parameters[0].Name, Is.EqualTo("target"));
        Assert.That(info.Parameters[0].Optional, Is.False);
        Assert.That(info.Parameters[0].Type, Is.EqualTo("any"));
    }
}
