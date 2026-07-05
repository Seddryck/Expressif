using Expressif.Functions.Array;
using Expressif.Predicates;
using Expressif.Predicates.Numeric;
using Expressif.Predicates.Text;
using Expressif;

namespace Expressif.Testing.Functions.Array;

public class FilterTest
{
    [Test]
    public void Evaluate_GreaterThan_Valid()
        => Assert.That(new Filter(() => new GreaterThan(() => 2)).Evaluate(new object[] { 1, 2, 3, 4 }), Is.EqualTo(new object?[] { 3, 4 }));

    [Test]
    public void Evaluate_Even_Valid()
        => Assert.That(new Filter(() => new Even()).Evaluate(new object[] { 1, 2, 3, 4, 5 }), Is.EqualTo(new object?[] { 2, 4 }));

    [Test]
    public void Evaluate_StartsWith_Valid()
        => Assert.That(new Filter(() => new StartsWith(() => "a")).Evaluate(new object[] { "alice", "bob", "anna" }), Is.EqualTo(new object?[] { "alice", "anna" }));

    [Test]
    public void Evaluate_AndPredicate_Valid()
        => Assert.That(new Filter(() => new Predication("greater-than(2) |AND less-than(5)")).Evaluate(new object[] { 1, 2, 3, 4, 5 }), Is.EqualTo(new object?[] { 3, 4 }));

    [Test]
    public void Evaluate_EmptyArray_EmptyArray_AndNoPredicateExecution()
    {
        var count = 0;
        var filter = new Filter(() => new DelegatedPredicate(x =>
        {
            count++;
            return true;
        }));

        var result = filter.Evaluate(System.Array.Empty<object>());

        Assert.That(result, Is.EqualTo(System.Array.Empty<object>()));
        Assert.That(count, Is.Zero);
    }

    [Test]
    public void Evaluate_SingleElementArray_True_Valid()
        => Assert.That(new Filter(() => new GreaterThan(() => 2)).Evaluate(new object[] { 10 }), Is.EqualTo(new object?[] { 10 }));

    [Test]
    public void Evaluate_SingleElementArray_False_EmptyArray()
        => Assert.That(new Filter(() => new GreaterThan(() => 2)).Evaluate(new object[] { 1 }), Is.EqualTo(System.Array.Empty<object>()));

    [Test]
    public void Evaluate_PredicateExecutedOncePerInputElement()
    {
        var count = 0;
        var filter = new Filter(() => new DelegatedPredicate(x =>
        {
            count++;
            return true;
        }));

        _ = filter.Evaluate(new object[] { 1, 2, 3, 4 });

        Assert.That(count, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_OutputItemsAreUnchanged()
    {
        var itemA = new object();
        var itemB = new object();
        var input = new object[] { itemA, itemB };

        var result = new Filter(() => new DelegatedPredicate(x => ReferenceEquals(x, itemB))).Evaluate(input) as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(ReferenceEquals(result![0], itemB), Is.True);
    }

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Filter(() => new Even()).Evaluate(10), Is.Null);

    [Test]
    public void Evaluate_Filter_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 1, 2, 3, 4 });
        var filter = new Filter(() => new Even());

        var result = filter.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { 2, 4 }));
        Assert.That(source.GetEnumeratorCalls, Is.EqualTo(1));
    }

    private sealed class DelegatedPredicate : IPredicate
    {
        private readonly Func<object?, bool> implementation;

        public DelegatedPredicate(Func<object?, bool> implementation)
            => this.implementation = implementation;

        public bool Evaluate(object? value)
            => implementation(value);

        object? Expressif.Functions.IFunction.Evaluate(object? value)
            => Evaluate(value);
    }

    private sealed class SingleEnumerationEnumerable : System.Collections.IEnumerable
    {
        private readonly object?[] values;
        public int GetEnumeratorCalls { get; private set; }

        public SingleEnumerationEnumerable(object?[] values)
            => this.values = values;

        public System.Collections.IEnumerator GetEnumerator()
        {
            GetEnumeratorCalls++;
            if (GetEnumeratorCalls > 1)
                throw new InvalidOperationException("Input was enumerated more than once.");
            return values.GetEnumerator();
        }
    }
}
