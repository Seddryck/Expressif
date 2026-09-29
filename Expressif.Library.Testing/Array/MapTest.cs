using Expressif.Library.Text.Casing;
using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Functions;
using Expressif.Library.Array;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Library.Temporal;
using Expressif.Library.Text;

namespace Expressif.Testing.Array;

public class MapTest
{
    [Test]
    public void Evaluate_MultiplyByTwo_Valid()
        => Assert.That(new Map(() => new Multiply(() => 2)).Evaluate(new object[] { 1, 2, 3 }), Is.EqualTo(new object?[] { 2m, 4m, 6m }));

    [Test]
    public void Evaluate_AddTen_Valid()
        => Assert.That(new Map(() => new Add(() => 10)).Evaluate(new object[] { 1, 2, 3 }), Is.EqualTo(new object?[] { 11m, 12m, 13m }));

    [Test]
    public void Evaluate_ChainedTransformations_Valid()
        => Assert.That(new Map(() => new ChainFunction([new Add(() => 10), new Multiply(() => 2)])).Evaluate(new object[] { 1, 2, 3 }), Is.EqualTo(new object?[] { 22m, 24m, 26m }));

    [Test]
    public void Evaluate_OutputCardinalityEqualsInputCardinality()
    {
        var input = new object[] { 1, 2, 3, 4 };

        var result = new Map(() => new Multiply(() => 2)).Evaluate(input) as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Length.EqualTo(input.Length));
    }

    [Test]
    public void Evaluate_EmptyArray_EmptyArray_AndNoTransformationExecution()
    {
        var count = 0;
        var map = new Map(() => new DelegatedFunction(x =>
        {
            count++;
            return x;
        }));

        var result = map.Evaluate(System.Array.Empty<object>());

        Assert.That(result, Is.EqualTo(System.Array.Empty<object>()));
        Assert.That(count, Is.Zero);
    }

    [Test]
    public void Evaluate_SingleElementArray_Valid()
        => Assert.That(new Map(() => new Multiply(() => 2)).Evaluate(new object[] { 10 }), Is.EqualTo(new object?[] { 20m }));

    [Test]
    public void Evaluate_TransformationMayChangeElementType_Valid()
        => Assert.That(new Map(() => new Upper()).Evaluate(new object[] { "alice", "bob", "charlie" }), Is.EqualTo(new object?[] { "ALICE", "BOB", "CHARLIE" }));

    [Test]
    public void Evaluate_TransformationExecutedOncePerInputElement()
    {
        var count = 0;
        var map = new Map(() => new DelegatedFunction(x =>
        {
            count++;
            return x;
        }));

        _ = map.Evaluate(new object[] { 1, 2, 3 });

        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
        => Assert.That(new Map(() => new DelegatedFunction(x => x)).Evaluate(10), Is.Null);

    [Test]
    public void Evaluate_Map_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 1, 2, 3 });
        var map = new Map(() => new Multiply(() => 2));

        var result = map.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { 2m, 4m, 6m }));
        Assert.That(source.GetEnumeratorCalls, Is.EqualTo(1));
    }

    [Conformance.Conformance]
    public void Map_Valid_Dictionary(object? input, string expression, string expected)
        => Assert.That(
            Expressif.Values.Formatting.ValueFormatter.Format(TestExpression.CreateClosed(expression).Evaluate(input)),
            Is.EqualTo(expected));

    [TestCase(false)]
    [TestCase(true)]
    public void Evaluate_Dictionary_VisitsUnchangedPairsOnceInOrder(bool empty)
    {
        var key = new Expressif.Values.TupleValue("BE", 2025m);
        var value = new object?[] { 100m, null };
        var input = new Expressif.Values.DictionaryValue(empty
            ? []
            : [new Expressif.Values.PairValue(key, value), new Expressif.Values.PairValue(null, null)]);
        var visited = new List<object?>();
        var map = new Map(() => new DelegatedFunction(pair =>
        {
            visited.Add(pair);
            return pair;
        }));

        var result = map.Evaluate(input);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<object?[]>());
            Assert.That(visited.Count, Is.EqualTo(input.Count));
            for (var index = 0; index < input.Count; index++)
                Assert.That(visited[index], Is.SameAs(input[index]));
            if (!empty)
            {
                Assert.That(input[0].Key, Is.SameAs(key));
                Assert.That(input[0].Value, Is.SameAs(value));
            }
        }
    }

    private sealed class DelegatedFunction : IFunction
    {
        private readonly Func<object?, object?> implementation;

        public DelegatedFunction(Func<object?, object?> implementation)
            => this.implementation = implementation;

        public object? Evaluate(object? value)
            => implementation(value);
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
