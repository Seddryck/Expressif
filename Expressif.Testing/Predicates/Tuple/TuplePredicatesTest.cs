using Expressif.Predicates.Tuple;
using Expressif.Values;

namespace Expressif.Testing.Predicates.Tuple;

public class TuplePredicatesTest
{
    [TestCase(0, 0, true)]
    [TestCase(2, 2, true)]
    [TestCase(3, 2, false)]
    public void HasArity_Evaluate_ReturnsExpected(int count, int expected, bool result)
        => Assert.That(new HasArity(() => expected).Evaluate(new TupleValue(new object?[count])), Is.EqualTo(result));

    [Test]
    public void HasArity_NegativeExpected_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new HasArity(() => -1).Evaluate(new TupleValue()));
}
