using Expressif.Functions.Array;

namespace Expressif.Testing.Functions.Array;

public class SelectionFunctionsTest
{
    [Test]
    public void Evaluate_FirstElements_Valid()
        => Assert.That(new FirstElements(() => 2).Evaluate(new object[] { 10, 20, 30, 40 }), Is.EqualTo(new object?[] { 10, 20 }));

    [Test]
    public void Evaluate_SkipFirstElements_Valid()
        => Assert.That(new SkipFirstElements(() => 2).Evaluate(new object[] { 10, 20, 30, 40 }), Is.EqualTo(new object?[] { 30, 40 }));

    [Test]
    public void Evaluate_LastElements_Valid()
        => Assert.That(new LastElements(() => 2).Evaluate(new object[] { 10, 20, 30, 40 }), Is.EqualTo(new object?[] { 30, 40 }));

    [Test]
    public void Evaluate_SkipLastElements_Valid()
        => Assert.That(new SkipLastElements(() => 2).Evaluate(new object[] { 10, 20, 30, 40 }), Is.EqualTo(new object?[] { 10, 20 }));

    [Test]
    public void Evaluate_SliceElements_Valid()
        => Assert.That(new SliceElements(() => 1, () => 4).Evaluate(new object[] { 10, 20, 30, 40, 50 }), Is.EqualTo(new object?[] { 20, 30, 40 }));

    [Test]
    public void Evaluate_EmptyArray_EmptyArray()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new FirstElements(() => 3).Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new SkipFirstElements(() => 3).Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new LastElements(() => 3).Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new SkipLastElements(() => 3).Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new SliceElements(() => 1, () => 3).Evaluate(System.Array.Empty<object>()), Is.EqualTo(System.Array.Empty<object>()));
        });
    }

    [Test]
    public void Evaluate_CountGreaterThanLength_SafeHandling()
    {
        var input = new object[] { 10, 20 };

        Assert.Multiple(() =>
        {
            Assert.That(new FirstElements(() => 5).Evaluate(input), Is.EqualTo(new object?[] { 10, 20 }));
            Assert.That(new SkipFirstElements(() => 5).Evaluate(input), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new LastElements(() => 5).Evaluate(input), Is.EqualTo(new object?[] { 10, 20 }));
            Assert.That(new SkipLastElements(() => 5).Evaluate(input), Is.EqualTo(System.Array.Empty<object>()));
        });
    }

    [Test]
    public void Evaluate_NegativeArguments_Null()
    {
        var input = new object[] { 1, 2, 3 };

        Assert.Multiple(() =>
        {
            Assert.That(new FirstElements(() => -1).Evaluate(input), Is.Null);
            Assert.That(new SkipFirstElements(() => -1).Evaluate(input), Is.Null);
            Assert.That(new LastElements(() => -1).Evaluate(input), Is.Null);
            Assert.That(new SkipLastElements(() => -1).Evaluate(input), Is.Null);
            Assert.That(new SliceElements(() => -1, () => 2).Evaluate(input), Is.Null);
            Assert.That(new SliceElements(() => 1, () => -2).Evaluate(input), Is.Null);
        });
    }

    [Test]
    public void Evaluate_SliceElements_Boundaries()
    {
        var input = new object[] { 10, 20, 30 };

        Assert.Multiple(() =>
        {
            Assert.That(new SliceElements(() => 1, () => 1).Evaluate(input), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new SliceElements(() => 3, () => 1).Evaluate(input), Is.EqualTo(System.Array.Empty<object>()));
            Assert.That(new SliceElements(() => 1, () => 10).Evaluate(input), Is.EqualTo(new object?[] { 20, 30 }));
        });
    }

    [Test]
    public void Evaluate_NonEnumerableInput_Null()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new FirstElements(() => 1).Evaluate(10), Is.Null);
            Assert.That(new SkipFirstElements(() => 1).Evaluate(10), Is.Null);
            Assert.That(new LastElements(() => 1).Evaluate(10), Is.Null);
            Assert.That(new SkipLastElements(() => 1).Evaluate(10), Is.Null);
            Assert.That(new SliceElements(() => 0, () => 1).Evaluate(10), Is.Null);
        });
    }

    [Test]
    public void Evaluate_SliceElements_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 10, 20, 30, 40, 50 });
        var expression = new SliceElements(() => 1, () => 4);

        var result = expression.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { 20, 30, 40 }));
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
