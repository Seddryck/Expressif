using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class TupleLiteralTest
{
    [Test]
    public void ExplicitSpread_ExpandsTupleInPlace()
        => Assert.That(
            Expression.CreateClosed("T(0, ...T(1, 2), 3)").Evaluate(null),
            Is.EqualTo(new TupleValue(0m, 1m, 2m, 3m)));

    [Test]
    public void MultipleSpreads_PreserveElementOrder()
        => Assert.That(
            Expression.CreateClosed("T(...T(1, 2), ...T(3, 4))").Evaluate(null),
            Is.EqualTo(new TupleValue(1m, 2m, 3m, 4m)));

    [Test]
    public void ImplicitSpread_ExpandsCurrentTupleInput()
        => Assert.That(
            Expression.Create("T(..., 3)").Evaluate(new TupleValue(1m, 2m)),
            Is.EqualTo(new TupleValue(1m, 2m, 3m)));

    [Test]
    public void ExplicitCurrentInputSpread_ExpandsCurrentTupleInput()
        => Assert.That(
            Expression.Create("T(...@_, 3)").Evaluate(new TupleValue(1m, 2m)),
            Is.EqualTo(new TupleValue(1m, 2m, 3m)));

    [TestCase("T(...#null)", "Spread argument cannot be null.")]
    [TestCase("T(...42)", "Spread argument must evaluate to a tuple.")]
    [TestCase("T(...{1, 2})", "Spread argument must evaluate to a tuple.")]
    public void InvalidSpread_ThrowsSpecificError(string source, string message)
        => Assert.That(
            () => Expression.CreateClosed(source).Evaluate(null),
            Throws.TypeOf<SpreadArgumentException>().With.Message.EqualTo(message));
}
