using Expressif.Functions.Array;

namespace Expressif.Testing.Functions.Array;

public class LagTest
{
    [Test]
    public void Evaluate_Valid()
        => Assert.That(new Lag().Evaluate(new object[] { 10, 20, 30 }), Is.EqualTo(new object?[] { null, 10, 20 }));

    [Test]
    public void Evaluate_OutputCardinalityEqualsInputCardinality()
    {
        var input = new object[] { 10, 20, 30, 40 };

        var result = new Lag().Evaluate(input) as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Length.EqualTo(input.Length));
    }

    [Test]
    public void Evaluate_EmptyArray_EmptyArray()
        => Assert.That(new Lag().Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));

    [Test]
    public void Evaluate_SingleElementArray_ArrayWithSingleNull()
        => Assert.That(new Lag().Evaluate(new object[] { 10 }), Is.EqualTo(new object?[] { null }));

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Lag().Evaluate(10), Is.Null);

    [Test]
    public void Evaluate_Lag_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 10, 20, 30 });
        var expression = new Lag();

        var result = expression.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { null, 10, 20 }));
        Assert.That(source.GetEnumeratorCalls, Is.EqualTo(1));
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
