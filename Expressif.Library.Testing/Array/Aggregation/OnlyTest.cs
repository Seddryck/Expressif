using Expressif.Introspection;
using Expressif.Library.Array.Aggregation;
using System.Collections;
using Expressif.Functions.Accumulation;
using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Predicates;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Aggregation;

[TestFixture]
public class OnlyTest
{
    [Conformance]
    public void Only_Selection(string value, string predicate, string accumulator, string? expected)
    {
        var result = expected is null ? null : TestExpression.CreateClosed(expected).Evaluate(null);
        var wrapper = new OnlyAccumulator(new PredicationFactory().Instantiate(predicate, new Context()), AccumulatorFactory.Instantiate(accumulator));
        wrapper.Initialize();
        foreach (var item in (IEnumerable)TestExpression.CreateClosed(value).Evaluate(null)!)
            wrapper.Accumulate(item);
        Assert.That(wrapper.GetValue(), Is.EqualTo(result));
        Assert.That(TestExpression.CreateClosed($"{value} | only({predicate}, {accumulator})").Evaluate(null), Is.EqualTo(result));
        Assert.That(TestExpression.CreateClosed($"{value} | fold(only({predicate}, {accumulator}))").Evaluate(null), Is.EqualTo(result));
    }

    [TestCase("only(accumulator := sum, predicate := is-even)", 6)]
    [TestCase("ONLY(is-even, sum) | increment", 7)]
    [TestCase("only(is-even, only(is-positive, sum))", 6)]
    [TestCase("fold(only(is-even, closest(3)))", 2)]
    [TestCase("fold(only(is-even, reduce(add~, 10)))", 16)]
    [TestCase("sum", 10)]
    [TestCase("count", 4)]
    public void Evaluate_Composition_Valid(string expression, int expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(new[] { 1, 2, 3, 4 }), Is.EqualTo(expected));

    [Test]
    public void Evaluate_TextAccumulator_PreservesOrder()
        => Assert.That(TestExpression.CreateClosed("{\"a\", #null, \"b\"} | only(is-not-null, concat(\"-\"))").Evaluate(null), Is.EqualTo("a-b"));

    [Test]
    public void Evaluate_Scan_LeavesStateUnchangedForRejectedItems()
        => Assert.That(TestExpression.Create("scan(only(is-even, count))").Evaluate(new[] { 1, 2, 3, 4 }), Is.EqualTo(new[] { 0, 1, 1, 2 }));

    [Test]
    public void Evaluate_LazyInput_EnumeratesOnce()
    {
        var visits = 0;
        IEnumerable<int> Items()
        {
            for (var i = 1; i <= 4; i++)
            {
                visits++;
                yield return i;
            }
        }
        Assert.That(TestExpression.Create("only(is-even, sum)").Evaluate(Items()), Is.EqualTo(6));
        Assert.That(visits, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_RecordPredicate_UsesEachItem()
        => Assert.That(TestExpression.CreateClosed("{{active := #true}, {active := #false}} | only(.active, count)").Evaluate(null), Is.EqualTo(1));

    [Test]
    public void Evaluate_NonBooleanPredicate_Throws()
        => Assert.That(() => TestExpression.Create("only(neutral, count)").Evaluate(new[] { 1 }), Throws.TypeOf<InvalidCastException>());

    [Test]
    public void Evaluate_AccumulatorFailure_Propagates()
        => Assert.That(() => TestExpression.Create("only(is-null, sum)").Evaluate(new object?[] { null }), Throws.TypeOf<InvalidCastException>());

    [Test]
    public void Evaluate_PredicateFailure_Propagates()
    {
        var wrapper = new OnlyAccumulator(new ThrowingPredicate(), new CountAccumulator());
        wrapper.Initialize();
        Assert.That(() => wrapper.Accumulate(1), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Initialize_ResetsWrappedState()
    {
        var wrapper = new OnlyAccumulator(new Expressif.Library.Numeric.Arithmetic.Even(), new CountAccumulator());
        wrapper.Initialize();
        wrapper.Accumulate(2);
        Assert.That(wrapper.GetValue(), Is.EqualTo(1));
        wrapper.Initialize();
        Assert.That(wrapper.GetValue(), Is.EqualTo(0));
    }

    [Test]
    public void Evaluate_ConcurrentCalls_IsolatesState()
    {
        var expression = TestExpression.Create("only(is-even, count)");
        Parallel.For(0, 100, i => Assert.That(expression.Evaluate(new[] { i, i + 1 }), Is.EqualTo(1)));
    }

    [Test]
    public void Describe_Only_ExposesParameters()
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(info => info.Name == "only");
        Assert.That(info.Parameters.Select(parameter => parameter.Type), Is.EqualTo(new[] { "predicate", "accumulator" }));
    }

    [Test]
    public void Bind_MissingAccumulator_Throws()
        => Assert.That(() => TestExpression.Create("only(is-even)"), Throws.TypeOf<MissingRequiredParameterException>());

    private sealed class ThrowingPredicate : IPredicate
    {
        public bool Evaluate(object? value) => throw new InvalidOperationException();
        object? Expressif.Functions.IFunction.Evaluate(object? value) => Evaluate(value);
    }
}
