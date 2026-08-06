using Expressif.Predicates;
using Expressif.Predicates.Array;
using Expressif.Testing.Conformance;
using SingleQuantifier = Expressif.Predicates.Array.Single;

namespace Expressif.Testing.Predicates.Array;

[TestFixture]
public class QuantifiersTest
{
    [Conformance]
    public void None_Valid_Predicate(object? value, string predicate, bool expected)
        => Assert.That(new None(() => new Predication(predicate)).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void All_Valid_Predicate(object? value, string predicate, bool expected)
        => Assert.That(new All(() => new Predication(predicate)).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Some_Valid_Predicate(object? value, string predicate, bool expected)
        => Assert.That(new Some(() => new Predication(predicate)).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Single_Valid_Predicate(object? value, string predicate, bool expected)
        => Assert.That(new SingleQuantifier(() => new Predication(predicate)).Evaluate(value), Is.EqualTo(expected));

    [TestCase("any(even)", true)]
    [TestCase("every(positive)", true)]
    public void Aliases_Valid(string code, bool expected)
        => Assert.That(new Predication(code).Evaluate(new object?[] { 2, 4 }), Is.EqualTo(expected));

    [Test]
    public void Some_ComposedPredicate_Valid()
        => Assert.That(
            new Predication("some(even |AND greater-than(2))").Evaluate(new object?[] { 1, 2, 3, 4 }),
            Is.True);

    [Test]
    public void None_StopsAfterFirstMatch()
    {
        var evaluated = new List<object?>();
        var quantifier = new None(() => new DelegatedPredicate(value =>
        {
            evaluated.Add(value);
            return Equals(value, 2);
        }));

        Assert.That(quantifier.Evaluate(new object?[] { 1, 2, 3 }), Is.False);
        Assert.That(evaluated, Is.EqualTo(new object?[] { 1, 2 }));
    }

    [Test]
    public void All_StopsAfterFirstNonMatch()
    {
        var evaluated = new List<object?>();
        var quantifier = new All(() => new DelegatedPredicate(value =>
        {
            evaluated.Add(value);
            return !Equals(value, 2);
        }));

        Assert.That(quantifier.Evaluate(new object?[] { 1, 2, 3 }), Is.False);
        Assert.That(evaluated, Is.EqualTo(new object?[] { 1, 2 }));
    }

    [Test]
    public void Some_StopsAfterFirstMatch()
    {
        var evaluated = new List<object?>();
        var quantifier = new Some(() => new DelegatedPredicate(value =>
        {
            evaluated.Add(value);
            return Equals(value, 2);
        }));

        Assert.That(quantifier.Evaluate(new object?[] { 1, 2, 3 }), Is.True);
        Assert.That(evaluated, Is.EqualTo(new object?[] { 1, 2 }));
    }

    [Test]
    public void Single_StopsAfterSecondMatch()
    {
        var evaluated = new List<object?>();
        var quantifier = new SingleQuantifier(() => new DelegatedPredicate(value =>
        {
            evaluated.Add(value);
            return Equals(value, 2);
        }));

        Assert.That(quantifier.Evaluate(new object?[] { 1, 2, 3, 2, 4 }), Is.False);
        Assert.That(evaluated, Is.EqualTo(new object?[] { 1, 2, 3, 2 }));
    }

    [Test]
    public void EmptyArray_DoesNotEvaluatePredicate()
    {
        var count = 0;
        var predicate = new DelegatedPredicate(_ =>
        {
            count++;
            return true;
        });

        Assert.Multiple(() =>
        {
            Assert.That(new None(() => predicate).Evaluate(System.Array.Empty<object?>()), Is.True);
            Assert.That(new All(() => predicate).Evaluate(System.Array.Empty<object?>()), Is.True);
            Assert.That(new Some(() => predicate).Evaluate(System.Array.Empty<object?>()), Is.False);
            Assert.That(new SingleQuantifier(() => predicate).Evaluate(System.Array.Empty<object?>()), Is.False);
            Assert.That(count, Is.Zero);
        });
    }

    [Test]
    public void NullElement_IsEvaluated()
        => Assert.That(
            new Some(() => new DelegatedPredicate(value => value is null)).Evaluate(new object?[] { null, 1, 2 }),
            Is.True);

    [Test]
    public void Some_EnumeratesProgressiveInputOnlyUntilResultIsKnown()
    {
        var source = new ThrowAfterMatchEnumerable();

        Assert.That(new Some(() => new DelegatedPredicate(value => Equals(value, 2))).Evaluate(source), Is.True);
    }

    private sealed class DelegatedPredicate(Func<object?, bool> implementation) : IPredicate
    {
        public bool Evaluate(object? value)
            => implementation(value);

        object? Expressif.Functions.IFunction.Evaluate(object? value)
            => Evaluate(value);
    }

    private sealed class ThrowAfterMatchEnumerable : System.Collections.IEnumerable
    {
        public System.Collections.IEnumerator GetEnumerator()
        {
            yield return 1;
            yield return 2;
            throw new InvalidOperationException("Enumeration continued after the quantifier result was known.");
        }
    }
}
