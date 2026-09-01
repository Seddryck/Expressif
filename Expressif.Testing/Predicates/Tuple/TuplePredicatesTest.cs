using Expressif.Predicates.Tuple;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Predicates.Tuple;

public class TuplePredicatesTest
{
    [Conformance]
    public void IsTuple_Valid(object? value, bool expected)
        => Assert.That(new IsTuple().Evaluate(ParseValue(value)), Is.EqualTo(expected));

    [Test]
    public void IsTuple_Evaluate_ChecksRuntimeKind()
        => Assert.Multiple(() =>
        {
            Assert.That(new IsTuple().Evaluate(new TupleValue(1, "x")), Is.True);
            Assert.That(new IsTuple().Evaluate(new object?[] { 1, 2 }), Is.False);
            Assert.That(new IsTuple().Evaluate(42), Is.False);
        });

    [TestCase(0, 0, true)]
    [TestCase(2, 2, true)]
    [TestCase(3, 2, false)]
    public void HasArity_Evaluate_ReturnsExpected(int count, int expected, bool result)
        => Assert.That(new HasArity(() => expected).Evaluate(new TupleValue(new object?[count])), Is.EqualTo(result));

    [Conformance]
    public void HasArity_Valid(string value, int arity, bool expected)
        => Assert.That(new HasArity(() => arity).Evaluate(ParseTuple(value)), Is.EqualTo(expected));

    [Test]
    public void HasArity_NegativeExpected_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new HasArity(() => -1).Evaluate(new TupleValue()));

    private static object? ParseValue(object? value)
        => value is string text && text.StartsWith("T(", StringComparison.Ordinal)
            ? ParseTuple(text)
            : value;

    private static TupleValue ParseTuple(string value)
        => value == "T()" ? new Expressif.Values.Tuple() : (TupleValue)Expression.CreateClosed(value).Evaluate(null)!;
}
