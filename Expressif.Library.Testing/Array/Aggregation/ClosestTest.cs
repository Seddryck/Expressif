using Expressif.Introspection;
using Expressif.Library.Array.Aggregation;
using System.Collections;
using Expressif.Functions.Accumulation;
using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Library.Temporal;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Array.Aggregation;

[TestFixture]
public class ClosestTest
{
    [Conformance]
    public void Closest_Selection(string value, string target, string? expected)
    {
        var reference = TestExpression.CreateClosed(target).Evaluate(null);
        var session = new ClosestAccumulator(() => reference).CreateSession();
        foreach (var item in (IEnumerable)TestExpression.CreateClosed(value).Evaluate(null)!)
            session.Add(item);

        var result = expected is null ? null : TestExpression.CreateClosed(expected).Evaluate(null);
        Assert.That(session.Snapshot(), Is.EqualTo(result));
        Assert.That(TestExpression.CreateClosed($"{value} | closest({target})").Evaluate(null), Is.EqualTo(result));
    }

    [TestCase("closest(32)")]
    [TestCase("closest(target := 32)")]
    [TestCase("CLOSEST(32)")]
    [TestCase("closest(32) | add(2)", 32)]
    public void Evaluate_Binding_Valid(string expression, int expected = 30)
        => Assert.That(TestExpression.Create(expression).Evaluate(new[] { 10, 30, 50 }), Is.EqualTo(expected));

    [Test]
    public void Evaluate_TargetExpression_UsesContext()
    {
        var context = new Context(new Dictionary<string, object?> { ["target"] = 31 });
        var expression = TestExpression.Create("closest({@target | increment})", context);
        Assert.That(expression.Evaluate(new[] { 10, 30, 50 }), Is.EqualTo(30));
    }

    [Test]
    public void Evaluate_MissingTarget_Throws()
        => Assert.That(() => TestExpression.Create("closest"), Throws.TypeOf<MissingRequiredParameterException>());

    [Test]
    public void Evaluate_UnknownTargetName_Throws()
        => Assert.That(() => TestExpression.Create("closest(value := 32)"), Throws.TypeOf<UnknownParameterNameException>());

    [Test]
    public void CreateSession_AfterAccumulation_IsolatesStateAndRefreshesTarget()
    {
        object? target = 32;
        var aggregation = new ClosestAccumulator(() => target);
        var numericSession = aggregation.CreateSession();
        numericSession.Add(30);
        target = new DateOnly(2024, 1, 12);
        var temporalSession = aggregation.CreateSession();
        Assert.That(temporalSession.Snapshot(), Is.Null);
        var date = new DateOnly(2024, 1, 10);
        temporalSession.Add(date);
        Assert.That(temporalSession.Snapshot(), Is.EqualTo(date));
        target = null;
        var nullTargetSession = aggregation.CreateSession();
        nullTargetSession.Add(30);
        Assert.Multiple(() =>
        {
            Assert.That(numericSession.Snapshot(), Is.EqualTo(30));
            Assert.That(nullTargetSession.Snapshot(), Is.Null);
        });
    }

    [Test]
    public void Accumulate_CoercedInput_PreservesOriginalObject()
    {
        object input = 30;
        var session = new ClosestAccumulator(() => 32m).CreateSession();
        session.Add(input);
        session.Add(50m);
        Assert.That(session.Snapshot(), Is.SameAs(input));
    }

    [Test]
    public void Evaluate_ConcurrentCalls_IsolatesState()
    {
        var expression = TestExpression.Create("closest(32)");
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
            var session = new ClosestAccumulator(() => target).CreateSession();
            foreach (var item in items)
                session.Add(item);
            Assert.That(session.Snapshot(), Is.SameAs(expected));
        }
    }

    [Test]
    public void Accumulate_Overflow_MatchesSubtract()
    {
        var session = new ClosestAccumulator(() => decimal.MinValue).CreateSession();
        Assert.That(() => new Subtract(() => decimal.MinValue).Evaluate(decimal.MaxValue), Throws.TypeOf<OverflowException>());
        Assert.That(() => session.Add(decimal.MaxValue), Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void Describe_Closest_ExposesRequiredTarget()
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(info => info.Name == "closest");
        Assert.That(info.Scope, Is.EqualTo("array/aggregation"));
        Assert.That(info.Summary, Is.EqualTo("Returns the first non-null input value with the smallest absolute distance to the target."));
        Assert.That(info.Parameters, Has.Count.EqualTo(1));
        Assert.That(info.Parameters[0].Name, Is.EqualTo("target"));
        Assert.That(info.Parameters[0].Optional, Is.False);
        Assert.That(info.Parameters[0].Type, Is.EqualTo("any"));
    }
}
