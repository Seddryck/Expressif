using Expressif.Accumulators;
using Expressif.Functions.Array;

namespace Expressif.Testing.Functions.Array;

public class ScanTest
{
    [Test]
    public void Evaluate_SumAccumulator_Valid()
        => Assert.That(new Scan(() => new SumAccumulator()).Evaluate(new object[] { 1, 2, 3 }), Is.EqualTo(new object?[] { 1m, 3m, 6m }));

    [Test]
    public void Evaluate_OutputCardinalityEqualsInputCardinality()
    {
        var input = new object[] { 1, "2", true, 4m };

        var result = new Scan(() => new CountAccumulator()).Evaluate(input) as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Length.EqualTo(input.Length));
    }

    [Test]
    public void Evaluate_EmptyArray_EmptyArray()
        => Assert.That(new Scan(() => new SumAccumulator()).Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Scan(() => new SumAccumulator()).Evaluate(10), Is.Null);

    [Test]
    public void Evaluate_Scan_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 1, 2, 3 });
        var expression = new Scan(() => new SumAccumulator());

        var result = expression.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { 1m, 3m, 6m }));
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
