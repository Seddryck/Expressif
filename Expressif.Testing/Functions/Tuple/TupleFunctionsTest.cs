using Expressif.Functions.Tuple;
using Expressif.Values;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Tuple;

public class TupleFunctionsTest
{
    [Conformance]
    public void Arity_Valid(string value, int expected)
        => Assert.That(
            new Arity().Evaluate(ParseTuple(value)),
            Is.EqualTo(expected));

    private static TupleValue ParseTuple(string value)
    {
        var source = value.Trim('"').Replace('{', '(').Replace('}', ')');
        return source == "T()" ? new Expressif.Values.Tuple() : (TupleValue)Expression.CreateClosed(source).Evaluate(null)!;
    }

    [Test]
    public void Evaluate_Accessors_Valid()
    {
        var tuple = new TupleValue(10, 20, 30);
        Assert.Multiple(() =>
        {
            Assert.That(new TupleFirst().Evaluate(tuple), Is.EqualTo(10));
            Assert.That(new TupleSecond().Evaluate(tuple), Is.EqualTo(20));
            Assert.That(new TupleAt(() => 2).Evaluate(tuple), Is.EqualTo(30));
            Assert.That(new TupleAt(() => 3).Evaluate(tuple), Is.Null);
            Assert.That(new TupleFirst().Evaluate(new object[] { 10, 20 }), Is.Null);
        });
    }

    [TestCase("T(10, 20, 30) | $^1", 30)]
    [TestCase("T(10, 20, 30) | $^2", 20)]
    [TestCase("T(10, 20, 30) | $^3", 10)]
    [TestCase("T(10, 20, 30) | $^0", null)]
    [TestCase("T(10, 20, 30) | $^4", null)]
    public void Evaluate_FromEndProjection_ReturnsExpected(string expression, object? expected)
        => Assert.That(Expression.CreateClosed(expression).Evaluate(null), Is.EqualTo(expected));
}
