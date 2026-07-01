namespace Expressif.Testing.Functions.Aggregation;

public class BroadcastTest
{
    [Test]
    public void Evaluate_ArrayLiteralPipeBroadcastSum_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | broadcast(sum)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 6m, 6m, 6m }));
    }

    [Test]
    public void Evaluate_Broadcast_PreservesCardinality()
    {
        var expression = new ClosedExpression("{1,2,3,4} | broadcast(sum)");
        var result = expression.Evaluate() as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Length, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_EmptyArrayPipeBroadcast_EmptyArray()
    {
        var expression = new ClosedExpression("{} | broadcast(sum)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(Array.Empty<object?>()));
    }

    [Test]
    public void Evaluate_DirectAccumulatorSyntax_StillScalar()
    {
        var expression = new ClosedExpression("{1,2,3} | sum");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(6m));
    }

    [Test]
    public void Evaluate_Broadcast_EnumeratesInputOnce()
    {
        var source = new SingleEnumerationEnumerable(new object?[] { 1, 2, 3 });
        var expression = new Expression("broadcast(sum)");

        var result = expression.Evaluate(source);

        Assert.That(result, Is.EqualTo(new object?[] { 6m, 6m, 6m }));
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
